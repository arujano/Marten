using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace Marten
{
    public class Account
    {
        /// <summary>
        /// Determines if this account is the current account of the network session.
        /// </summary>
        private bool authenticated = false;

        public string DisplayName { get; private set; }
        public string Username { get; private set; }
        public string UserId { get; private set; }

        public static async Task<Account> FetchCurrent()
        {
            Nakama.IApiAccount apiAccount = await Network.Client.GetAccountAsync(Network.Session);

            return new()
            {
                authenticated = true,
                DisplayName = apiAccount.User.DisplayName,
                Username = apiAccount.User.Username,
                UserId = apiAccount.User.Id,
            };
        }

        public static async Task<Account> Fetch(string idOrUsername)
        {
            Nakama.IApiUsers apiUsers = await Network.Client.GetUsersAsync(Network.Session, [idOrUsername]);
            Nakama.IApiUser account = apiUsers.Users.First();

            if (account == null)
            {
                GD.PrintErr(string.Format("Could not fetch account of user '{0}'", idOrUsername));
                return null;
            }

            return new()
            {
                authenticated = false,
                DisplayName = account.DisplayName,
                Username = account.Username,
                UserId = account.Id,
            };
        }

        /// <summary>
        /// Fetches the account data from the server and writes it to this object.
        /// </summary>
        public async void Sync()
        {
            if (authenticated)
            {
                Nakama.IApiAccount apiAccount = await Network.Client.GetAccountAsync(Network.Session);
                DisplayName = apiAccount.User.DisplayName;
            }
            else
            {
                Nakama.IApiUsers apiUsers = await Network.Client.GetUsersAsync(Network.Session, [UserId]);
                Nakama.IApiUser account = apiUsers.Users.First();

                DisplayName = account.DisplayName;
            }
        }

        public async void SetDisplayName(string newName)
        {
            if (!authenticated)
            {
                GD.PrintErr(string.Format("Error setting username of user '{0}': account is not authenticated in network.", Username));
                return;
            }

            await Network.Client.UpdateAccountAsync(Network.Session, Username, newName);
            DisplayName = newName;
        }
    }
}