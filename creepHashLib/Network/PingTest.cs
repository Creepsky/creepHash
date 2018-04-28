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
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Common;

namespace creepHashLib.Network
{
    public class PingTest : PeriodicTask<(Uri, TimeSpan)>
    {
        private readonly IEnumerable<Uri> _uris;

        public PingTest(IEnumerable<Uri> uris, CancellationToken ctx, TimeSpan tick)
            : base(tick, ctx)
        {
            _uris = uris;
        }

        protected override (Uri, TimeSpan) Run() => GetBestPing(_uris).Result;

        public static TimeSpan SendPing(Uri uri)
        {
            var ping = new Ping();
            var stopWatch = Stopwatch.StartNew();
            ping.Send(uri.Host);
            return stopWatch.Elapsed;
        }

        public static Task<IDictionary<Uri, TimeSpan>> SendPing(IEnumerable<Uri> uris)
        {
            return Task.Run(() =>
            {
                return uris.ToDictionary(i => i, SendPing) as IDictionary<Uri, TimeSpan>;
            });
        }

        public static async Task<(Uri, TimeSpan)> GetBestPing(IEnumerable<Uri> ips)
        {
            var times = await SendPing(ips);

            (Uri ip, TimeSpan time) best = (null, TimeSpan.MaxValue);

            foreach (var j in times)
                if (best.ip == null || best.time > j.Value)
                    best = (j.Key, j.Value);

            return best;
        }
    }
}