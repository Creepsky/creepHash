using System.Collections.Generic;
using System.Linq;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Mining;
using MultiCryptoToolLib.Mining.Hardware;

namespace MultiCryptoToolLib.Benchmark.File
{
    public interface IBenchmarkFile : ILoader<IDictionary<Hardware, IDictionary<Algorithm, HashRate>>>
    {
        IDictionary<Hardware, IDictionary<Algorithm, HashRate>> HashRates { get; set; }

        string Path { get; }

        void Save();
    }

    public static class BenchmarkFileHelper
    {
        public static IEnumerable<(Algorithm algorithm, Hardware hardware)> GetUnfinishedAlgorithms(
            this IBenchmarkFile file, Miner miner, IEnumerable<Hardware> miningHardware) =>
            from hw in miner.HardwareTypes
            from h in miningHardware.Where(i => i.Type == hw)
            from a in miner.Algorithms.Where(i => !file.IsComplete(i, h))
            select (algorithm: a, hardware: h);

        public static bool IsComplete(this IBenchmarkFile file, Algorithm algorithm, Hardware miningHardware) =>
            file.HashRates.ContainsKey(miningHardware) && file.HashRates[miningHardware].ContainsKey(algorithm);

        public static bool IsComplete(this IBenchmarkFile file, IEnumerable<Algorithm> algorithms, Hardware miningHardware)
        {
            if (!file.HashRates.ContainsKey(miningHardware))
                return false;

            return !algorithms.Except(file.HashRates[miningHardware].Keys).Any();
        }

        public static bool IsComplete(this IBenchmarkFile file, IEnumerable<Algorithm> algorithms, IEnumerable<Hardware> miningHardware) =>
            !miningHardware.Except(file.HashRates.Select(i => i.Key)).Any() &&
            miningHardware.All(hardware => algorithms.All(algorithm => file.HashRates[hardware].ContainsKey(algorithm)));
    }
}