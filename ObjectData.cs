namespace Marten
{
    /// <summary>
    /// Represents the owner of a network-synchronized object.
    /// </summary>
    public class NetOwner
    {
        public string UserID = "";

        /// <summary>
        /// Checks if the current network client is the owner of this object.<br/>
        /// <b>NOTE:</b> Returns false if the client is not connected to a server.
        /// </summary>
        public bool IsOwner()
        {
            return Network.Connected && Network.Account.UserId == UserID;
        }
    }
}