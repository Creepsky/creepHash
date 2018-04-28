using System;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Mining;
using MultiCryptoToolLib.Mining.Hardware;

namespace MultiCryptoToolLib.Benchmark
{
    public abstract class Benchmark : ILoaderAsync<HashRate>
    {
        public Miner Miner { get; }
        public string Algorithm { get; }
        public Hardware MiningHardware { get; }

        protected Benchmark(Miner miner, string algorithm, Hardware miningHardware)
        {
            Miner = miner;
            Algorithm = algorithm;
            MiningHardware = miningHardware;
        }

        public HashRate Load(CancellationToken ctx)
        {
            return Run(ctx);
        }

        public Task<HashRate> LoadAsync(CancellationToken ctx)
        {
            return RunAsync(ctx);
        }

        protected abstract HashRate Run(CancellationToken cancel);
        protected abstract Task<HashRate> RunAsync(CancellationToken cancel);

        public static Benchmark Create(Miner miner, string algorithm, Hardware hardware)
        {
            if (miner.Name == "ccminer")
                return new CcminerBenchmark(algorithm, hardware);

            if (miner.Name == "ethminer")
                return new EthminerBenchmark(algorithm, hardware);

            throw new ArgumentException($"No benchmark found for {miner}");
        }
    }
}
