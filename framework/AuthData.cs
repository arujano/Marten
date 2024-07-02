using System.Threading.Tasks;
using Godot;

namespace Marten
{
    /// <summary>
    /// Contains the data necessary for authentication.
    /// Should be discarded after the session has been authenticated.
    /// </summary>
    public class AuthData
    {
        public string Username { get; private set; }
        public string Password { get; private set; }

        public AuthData(string username, string password)
        {
            Username = username;
            Password = password;
        }

        internal async Task<Nakama.ISession> Authenticate(Nakama.Client client)
        {
            try { return await client.AuthenticateCustomAsync(Password, Username); }
            catch (Nakama.ApiResponseException ex)
            {
                GD.PrintErr(string.Format("Error authenticating account: {0}: {1}", ex.StatusCode, ex.Message));
                return null;
            }
        }
    }
}