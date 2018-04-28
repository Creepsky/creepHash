using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;
using MultiCryptoToolLib.Mining;
using MultiCryptoToolLib.Mining.Hardware;

namespace MultiCryptoToolLib.Benchmark
{
    public class EthminerBenchmark : Benchmark
    {
        public EthminerBenchmark(string algorithm, Hardware miningHardware)
            : base(Miner.FromString("ethminer"), algorithm, miningHardware)
        {
        }

        protected override HashRate Run(CancellationToken cancel)
        {
            var hashRate = new HashRate();

            string additionalParameter;

            switch (MiningHardware.Type)
            {
                case HardwareType.OpenCl:
                    additionalParameter = $"-G --opencl-platform {MiningHardware.PlatformIndex} --opencl-device {MiningHardware.Index}";
                    break;
                case HardwareType.Cuda:
                    additionalParameter = $"-U --cuda-devices {MiningHardware.Index}";
                    break;
                default:
                    additionalParameter = "";
                    break;
            }

            var lines = ProcessHelper.ReadLines(Miner.Path, $"--benchmark 5159080 {additionalParameter}", cancel);

            foreach (var line in lines)
            {
                Logger.Debug(line);
                var match = Regex.Match(line, @".*inner mean:\s(\S+)\sH/s");

                if (match.Success)
                {
                    var hashRateValue = double.Parse(match.Groups[1].Value);
                    hashRate = new HashRate(hashRateValue, Metric.Unit);
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