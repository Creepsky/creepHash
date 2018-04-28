using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;

namespace MultiCryptoToolLib.Network
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