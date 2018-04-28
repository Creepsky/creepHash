/*
 * Copyright 2018 Creepsky
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *  http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;
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
                try
                {
                    var j = (JObject) i.Value;
                    infos.Add(Coin.FromString(i.Name).CreateInfo(j.Value<int>("port"), j.Value<double>("profitability")));
                }
                catch (Exception)
                {
                    Logger.Debug($"Unknown coin {i.Name}");
                }
            }

            return infos;
        }
    }
}