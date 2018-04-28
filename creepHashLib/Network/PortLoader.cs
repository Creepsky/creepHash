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
using creepHashLib.Common.Logging;
using Newtonsoft.Json.Linq;
using Console = System.Console;

namespace creepHashLib.Network
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