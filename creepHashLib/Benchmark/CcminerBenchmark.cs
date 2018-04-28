using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;
using MultiCryptoToolLib.Mining;
using MultiCryptoToolLib.Mining.Hardware;

namespace MultiCryptoToolLib.Benchmark
{
    public class CcminerBenchmark : Benchmark
    {
        public CcminerBenchmark(string algorithm, Hardware miningHardware)
            : base(Miner.FromString("ccminer"), algorithm, miningHardware)
        {
        }

        protected override HashRate Run(CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested)
                cancel.ThrowIfCancellationRequested();

            var hashRate = new HashRate();

            var lines = ProcessHelper.ReadLines(
                Miner.Path,
                $"--algo={Algorithm} --benchmark --no-color --quiet --devices={MiningHardware.Index} --time-limit 15",
                cancel
            );

            foreach (var line in lines)
            {
                Logger.Debug(line);

                var match = Regex.Match(line, @"\G\[.*\]\s*Benchmark:\s*(\d+.\d+)\s*(.*)");

                if (match.Success)
                {
                    var hashRateString = match.Groups[1].Value;
                    var metric = match.Groups[2].Value;

                    try
                    {
                        var hashRateValue = double.Parse(hashRateString, CultureInfo.InvariantCulture);
                        hashRate = new HashRate(hashRateValue, metric);
                    }
                    catch (Exception e)
                    {
                        Logger.Exception($"Could not read hashrate of {Algorithm} -> {hashRateString}", e);
                    }
                }
            }

            return hashRate;
        }

        protected override Task<HashRate> RunAsync(CancellationToken cancel)
        {
            return Task.FromResult(Run(cancel));
        }
    }
}
