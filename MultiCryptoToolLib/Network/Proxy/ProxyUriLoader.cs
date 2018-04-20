using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network.Proxy
{
    public class ProxyUriLoader : ILoaderAsync<IList<Uri>>
    {
        private readonly Uri _uri;

        public ProxyUriLoader(Uri uri)
        {
            _uri = uri;
        }

        public IList<Uri> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = w.DownloadString(_uri);
                return ParseIps(content);
            }
        }

        public async Task<IList<Uri>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = await w.DownloadStringTaskAsync(_uri);
                return ParseIps(content);
            }
        }

        private static IList<Uri> ParseIps(string content)
        {
            var json = JArray.Parse(content);
            var collection = new List<Uri>();

            foreach (var j in json)
                collection.Add(new Uri(j.Value<string>()));

            return collection;
        }
    }
}