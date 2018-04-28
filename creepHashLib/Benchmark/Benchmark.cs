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
