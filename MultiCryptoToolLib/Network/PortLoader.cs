using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;
using Newtonsoft.Json.Linq;
using Console = System.Console;

namespace MultiCryptoToolLib.Network
{
    public class PortLoader : ILoaderAsync<IDictionary<Mining.Coin, int>>
    {
        private readonly Uri _uri;

        public PortLoader(Uri uri)
        {
            _uri = uri;
        }

        public IDictionary<Mining.Coin, int> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParsePorts(w.DownloadString(_uri));
            }
        }

        public async Task<IDictionary<Mining.Coin, int>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParsePorts(await w.DownloadStringTaskAsync(_uri));
            }
        }

        private static IDictionary<Mining.Coin, int> ParsePorts(string content)
        {
            var json = JObject.Parse(content);
            var ports = new Dictionary<Mining.Coin, int>();
            foreach (var j in json)
                try
                {
                    ports.Add(Mining.Coin.FromString(j.Key), j.Value.Value<int>("port"));
                }
                catch (Exception)
                {
                    Logger.Debug($"Unknown coin {j.Key}");
                }
            return ports;
        }
    }
}