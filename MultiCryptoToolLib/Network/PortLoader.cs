using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network
{
    public class PortLoader : ILoaderAsync<IDictionary<Mining.Algorithm, int>>
    {
        private readonly Uri _uri;

        public PortLoader(Uri uri)
        {
            _uri = uri;
        }

        public IDictionary<Mining.Algorithm, int> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParsePorts(w.DownloadString(_uri));
            }
        }

        public async Task<IDictionary<Mining.Algorithm, int>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParsePorts(await w.DownloadStringTaskAsync(_uri));
            }
        }

        private static IDictionary<Mining.Algorithm, int> ParsePorts(string content)
        {
            var json = JObject.Parse(content);
            var ports = new Dictionary<Mining.Algorithm, int>();
            foreach (var j in json)
                ports.Add(Mining.Algorithm.FromString(j.Key), j.Value.Value<int>());
            return ports;
        }
    }
}