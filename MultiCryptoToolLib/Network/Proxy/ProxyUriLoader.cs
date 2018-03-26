using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network.Proxy
{
    public class ProxyUriLoader : ILoaderAsync<IList<IPAddress>>
    {
        private readonly Uri _uri;

        public ProxyUriLoader(Uri uri)
        {
            _uri = uri;
        }

        public IList<IPAddress> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = w.DownloadString(_uri);
                return ParseIps(content);
            }
        }

        public async Task<IList<IPAddress>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = await w.DownloadStringTaskAsync(_uri);
                return ParseIps(content);
            }
        }

        private static IList<IPAddress> ParseIps(string content)
        {
            var json = JArray.Parse(content);
            var collection = new List<IPAddress>();

            foreach (var j in json)
                collection.Add(IPAddress.Parse(j.Value<string>()));

            return collection;
        }
    }
}