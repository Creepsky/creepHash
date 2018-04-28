using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Mining;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network.Algorithm
{
    public class AlgorithmWebLoader : IAlgorithLoaderAsync
    {
        private readonly Uri _uri;

        public AlgorithmWebLoader(Uri uri)
        {
            _uri = uri;
        }

        public IDictionary<string, Uri> Load(CancellationToken cancel)
        {
            using (var w = new WebClient())
            {
                var content = w.DownloadString(_uri);
                var json = JArray.Parse(content);
                var jsonLoader = new AlgorithmJsonLoader(json);
                return jsonLoader.Load(cancel);
            }
        }

        public async Task<IDictionary<string, Uri>> LoadAsync(CancellationToken cancel)
        {
            using (var w = new WebClient())
            {
                var content = await w.DownloadStringTaskAsync(_uri);
                var json = JArray.Parse(content);
                var jsonLoader = new AlgorithmJsonLoader(json);
                return jsonLoader.Load(cancel);
            }
        }
    }
}
