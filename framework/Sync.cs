using System.Text;
using Newtonsoft.Json;

namespace Marten
{
    internal delegate void SyncStorageHandler(NetStorageSyncData data);

    /// <summary>
    /// Receives match state and routes it to specific events.
    /// </summary>
    public static class Dispatcher
    {
        internal static event SyncStorageHandler SyncStorage;

        internal static void ReceivedMatchState(Nakama.IMatchState state)
        {
            switch (state.OpCode)
            {
                case NetCodes.SyncStorage:
                    string encodedData = Encoding.UTF8.GetString(state.State);
                    SyncStorage.Invoke(JsonConvert.DeserializeObject<NetStorageSyncData>(encodedData));
                    break;
                default:
                    break;
            }
        }
    }
}