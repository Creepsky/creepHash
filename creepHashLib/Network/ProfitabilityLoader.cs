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
using creepHashLib.Common;
using creepHashLib.Mining;
using Newtonsoft.Json.Linq;

namespace creepHashLib.Network
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