using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;

namespace MultiCryptoToolLib.Network
{
    public class LoginManager
    {
        private readonly Uri _uri;
        private readonly CancellationToken _ctx;

        public string Username { get; private set; }
        public string Address { get; private set; } = string.Empty;
        public bool IsLoggedIn => !string.IsNullOrWhiteSpace(Address);

        public LoginManager(Uri uri, CancellationToken ctx)
        {
            _uri = uri;
            _ctx = ctx;
        }

        public async Task<bool> Login(string user, string password)
        {
            var response = await new HttpClient().PostAsync(new Uri(_uri, "login"), new FormUrlEncodedContent(
                new Dictionary<string, string>
                {
                    {"username", user},
                    {"password", password.ToSha256()}
                }), _ctx);

            var message = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Logger.Error(message);
                return false;
            }

            Address = await response.Content.ReadAsStringAsync();

            if (!IsLoggedIn)
                return false;

            Username = user;
            return true;
        }

        public void Logout()
        {
            Address = string.Empty;
            Username = string.Empty;
        } 
    }
}