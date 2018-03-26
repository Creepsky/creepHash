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
    public class PingTest : PeriodicTask<(IPAddress, TimeSpan)>
    {
        private readonly IEnumerable<IPAddress> _ips;

        public PingTest(IEnumerable<IPAddress> ips, CancellationToken ctx, TimeSpan tick)
            : base(tick, ctx)
        {
            _ips = ips;
        }

        protected override (IPAddress, TimeSpan) Run() => GetBestPing(_ips).Result;

        public static TimeSpan SendPing(IPAddress ip)
        {
            var ping = new Ping();
            var stopWatch = Stopwatch.StartNew();
            ping.Send(ip);
            return stopWatch.Elapsed;
        }

        public static Task<IDictionary<IPAddress, TimeSpan>> SendPing(IEnumerable<IPAddress> ips)
        {
            return Task.Run(() =>
            {
                return ips.ToDictionary(i => i, SendPing) as IDictionary<IPAddress, TimeSpan>;
            });
        }

        public static async Task<(IPAddress, TimeSpan)> GetBestPing(IEnumerable<IPAddress> ips)
        {
            var times = await SendPing(ips);

            (IPAddress ip, TimeSpan time) best = (null, TimeSpan.MaxValue);

            foreach (var j in times)
                if (best.ip == null || best.time > j.Value)
                    best = (j.Key, j.Value);

            return best;
        }
    }
}