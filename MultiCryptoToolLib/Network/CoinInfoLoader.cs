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
    public class CoinInfoLoader : ILoaderAsync<IList<Coin.Info>>
    {
        private readonly Uri _uri;

        public CoinInfoLoader(Uri uri)
        {
            _uri = uri;
        }

        public IList<Coin.Info> Load(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParseCoinInfos(w.DownloadString(_uri));
            }
        }

        public async Task<IList<Coin.Info>> LoadAsync(CancellationToken ctx)
        {
            using (var w = new WebClient())
            {
                return ParseCoinInfos(await w.DownloadStringTaskAsync(_uri));
            }
        }

        private static IList<Coin.Info> ParseCoinInfos(string text)
        {
            var json = JObject.Parse(text);
            var infos = new List<Coin.Info>();

            foreach (var i in json.Children<JProperty>())
            {
                var j = (JObject) i.Value;
                infos.Add(Coin.FromString(i.Name).CreateInfo(j.Value<int>("port"), j.Value<double>("profitability")));
            }

            return infos;
        }
    }
}