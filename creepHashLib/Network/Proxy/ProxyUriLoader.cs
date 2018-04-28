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