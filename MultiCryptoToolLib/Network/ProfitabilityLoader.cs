using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Mining;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Network
{
    public class ProfitabilityLoader : ILoaderAsync<IDictionary<Coin, double>>
    {
        private readonly Uri _uri;

        public ProfitabilityLoader(Uri uri)
        {
            _uri = uri;
        }

        public IDictionary<Coin, double> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = w.DownloadString(_uri);
                return ParseProfitabilities(content);
            }
        }

        public async Task<IDictionary<Coin, double>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                var content = await w.DownloadStringTaskAsync(_uri);
                return ParseProfitabilities(content);
            }
        }

        private static IDictionary<Coin, double> ParseProfitabilities(string content)
        {
            var json = JObject.Parse(content);
            var profitabilities = new Dictionary<Coin, double>();

            foreach (var i in json)
                profitabilities.Add(Coin.FromString(i.Key), i.Value.Value<double>());

            return profitabilities;
        }
    }
}