using Godot;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Marten
{
    public class Match
    {
        private Nakama.IMatch match;

        public static async Task<Match> Create()
        {
            if (!Network.Connected)
                return null;

            try
            {
                return new()
                {
                    match = await Network.Socket.CreateMatchAsync()
                };
            }
            catch (Nakama.ApiResponseException ex)
            {
                GD.PrintErr(string.Format("Error creating match: {0}: {1}", ex.StatusCode, ex.Message));
                return null;
            }
        }

        public static async Task<Match> Join(string id)
        {
            if (!Network.Connected)
                return null;

            try
            {
                return new()
                {
                    match = await Network.Socket.JoinMatchAsync(id)
                };
            }
            catch (Nakama.ApiResponseException ex)
            {
                GD.PrintErr(string.Format("Error joining match: {0}: {1}", ex.StatusCode, ex.Message));
                return null;
            }
        }

        public string Id()
        {
            return match.Id;
        }

        public async Task<List<Account>> Players()
        {
            List<Account> players = [];

            foreach (Nakama.IUserPresence presence in match.Presences)
                players.Add(await Account.Fetch(presence.UserId));

            return players;
        }
    }
}