using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Marten
{
    /// <summary>
    /// Interface for all types which can be stored on the server storage.
    /// </summary>
    public interface IStorageData
    {
        /// <summary>
        /// The default constructor for types which can be stored in the server.
        /// </summary>
        /// <returns>
        /// An instance of this type with default values attached.
        /// </returns>
        public static abstract IStorageData NetDefault();
    }

    /// <summary>
    /// Storage type for synchronizing storage objects over match state.
    /// </summary>
    internal class NetStorageSyncData
    {
        public Nakama.StorageObjectId Id;
        public string EncodedData;
    }

    public delegate void SyncedStorageHandler();

    /// <summary>
    /// Represents a data object contained in the server.
    /// Can optionally be synchronized automatically when the network is connected to a match.
    /// </summary>
    /// <typeparam name="T">The type of data this object contains.</typeparam>
    public class NetStorage<T> where T : IStorageData
    {
        public event SyncedStorageHandler SyncedStorage;

        public Nakama.StorageObjectId Id { get; private set; }
        public T Data { get; set; }
        /// <summary>Determines if this storage object should sync whenever it is written to.</summary>
        public bool SyncOnWrite { get; private set; }

        /// <summary>
        /// Fetches a storage object from the server.
        /// </summary>
        /// <param name="collection">The collection in which this data is saved in the server.</param>
        /// <param name="key">The name of this data in the server.</param>
        /// <param name="userId">The ID of the user that owns this storage object.</param>
        /// <param name="syncOnWrite">Determines if this storage object should alert others whenever it is written to.</param>
        /// <returns>
        /// A `NetStorage<T>` object with data obtained from the server.<br/>
        /// <b>NOTE:</b> if the server does not have an object with the specified address, the returned data will be the default values.
        /// </returns>
        public static async Task<NetStorage<T>> Fetch(string collection, string key, string userId, bool syncOnWrite = true)
        {
            Nakama.StorageObjectId id = new()
            {
                Collection = collection,
                Key = key,
                UserId = userId,
            };

            Nakama.IApiStorageObjects result = await Network.Client.ReadStorageObjectsAsync(Network.Session, [id]);

            if (!result.Objects.Any())
                return new()
                {
                    Id = id,
                    Data = (T)T.NetDefault(),
                };

            id.Version = result.Objects.First().Version;

            return new()
            {
                Id = id,
                Data = JsonConvert.DeserializeObject<T>(result.Objects.First().Value),
                SyncOnWrite = syncOnWrite,
            };
        }

        /// <summary>
        /// Fetches the storage data from the server and rewrites it to this object.
        /// </summary>
        public async void Refetch()
        {
            Nakama.IApiStorageObjects result = await Network.Client.ReadStorageObjectsAsync(Network.Session, new[] { Id });

            if (!result.Objects.Any())
                return;

            Id.Version = result.Objects.First().Version;
            Data = JsonConvert.DeserializeObject<T>(result.Objects.First().Value);

            SyncedStorage.Invoke();
        }

        /// <summary>
        /// Writes the data of this storage object to the server.
        /// Will only write if this storage object belongs to the current network account.<br/>
        /// <b>NOTE:</b> always call this after updating the storage data!
        /// </summary>
        public async void Write()
        {
            if (Id.UserId != Network.Account.UserId)
                return;

            string encodedData = JsonConvert.SerializeObject(Data);

            Nakama.WriteStorageObject writeObject = new()
            {
                Collection = Id.Collection,
                Key = Id.Key,
                Value = encodedData,
                PermissionRead = 2,
                PermissionWrite = 1,
            };

            await Network.Client.WriteStorageObjectsAsync(Network.Session, [writeObject]);

            if (SyncOnWrite && Network.Match != null)
            {
                NetStorageSyncData data = new()
                {
                    Id = Id,
                    EncodedData = encodedData,
                };

                await Network.Socket.SendMatchStateAsync(Network.Match.Id(), NetCodes.SyncStorage, JsonConvert.SerializeObject(data));
            }
        }

        /// <summary>
        /// Called from `Marten.Dispatcher.SyncStorage` events.
        /// </summary>
        /// <param name="data">The data to be written on this object.</param>
        internal void Sync(NetStorageSyncData data)
        {
            if (data.Id.UserId != Id.UserId || data.Id.Collection != Id.Collection || data.Id.Key != Id.Key)
                return;

            Id.Version = data.Id.Version;
            Data = JsonConvert.DeserializeObject<T>(data.EncodedData);

            SyncedStorage.Invoke();
        }
    }
}