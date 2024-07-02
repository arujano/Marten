using System.Threading.Tasks;
using Godot;

namespace Marten
{
    public delegate void JoinedMatchHandler();
    public delegate void LeftMatchHandler();

    /// <summary>
    /// Contains all of the fundamental parts of the network code.
    /// </summary>
    public static class Network
    {
        public static event JoinedMatchHandler JoinedMatch;
        public static event LeftMatchHandler LeftMatch;

        public static bool Connected { get; private set; } = false;
        public static Nakama.Client Client { get; private set; } = null;
        public static Nakama.ISession Session { get; private set; } = null;
        public static Nakama.ISocket Socket { get; private set; } = null;
        public static Account Account { get; private set; } = null;
        public static Nakama.IMatch Match { get; private set; } = null;

        public static async Task<bool> Connect(string serverIp, AuthData authData, int port = 7350)
        {
            if (Connected)
            {
                GD.PrintErr("Error connecting to server: already connected!");
                return false;
            }

            try
            {
                Client = new("http", serverIp, port, "defaultkey");
                Session = await authData.Authenticate(Client);
                Socket = Nakama.Socket.From(Client);

                await Socket.ConnectAsync(Session, true);
                Socket.Closed += Disconnect;

                Account = await Account.FetchCurrent();

                Socket.ReceivedMatchState += Dispatcher.ReceivedMatchState;

                Connected = true;
            }
            catch (Nakama.ApiResponseException ex)
            {
                GD.PrintErr(string.Format("Error connecting to server: {0}: {1}", ex.StatusCode, ex.Message));
                return false;
            }

            return true;
        }

        public static void Disconnect()
        {
            Socket = null;
            Session = null;
            Client = null;
            Account = null;

            Connected = false;
        }
    }
}