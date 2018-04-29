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

using System.Collections.Generic;
using System.Linq;
using creepHashLib.Common;
using creepHashLib.Mining;
using creepHashLib.Mining.Hardware;

namespace creepHashLib.Benchmark.File
{
    public interface IBenchmarkFile : ILoader<IDictionary<Hardware, IDictionary<string, HashRate>>>
    {
        IDictionary<Hardware, IDictionary<string, HashRate>> HashRates { get; set; }

        string Path { get; }

        void Save();
    }

    public static class BenchmarkFileHelper
    {
        public static IEnumerable<(string algorithm, Hardware hardware)> GetUnfinishedAlgorithms(
            this IBenchmarkFile file, Miner miner, IEnumerable<Hardware> miningHardware) =>
            from hw in miner.HardwareTypes
            from h in miningHardware.Where(i => i.Type == hw)
            from a in miner.Algorithms.Where(i => !file.IsComplete(i, h))
            select (algorithm: a, hardware: h);

        public static bool IsComplete(this IBenchmarkFile file, string algorithm, Hardware miningHardware) =>
            file.HashRates.ContainsKey(miningHardware) && file.HashRates[miningHardware].ContainsKey(algorithm);

        public static bool IsComplete(this IBenchmarkFile file, IEnumerable<string> algorithms, Hardware miningHardware)
        {
            if (!file.HashRates.ContainsKey(miningHardware))
                return false;

            return !algorithms.Except(file.HashRates[miningHardware].Keys).Any();
        }

        public static bool IsComplete(this IBenchmarkFile file, IEnumerable<string> algorithms, IEnumerable<Hardware> miningHardware)
        {
            var enumerable = miningHardware.ToList();

            return !enumerable.Except(file.HashRates.Select(i => i.Key)).Any() &&
                   enumerable.All(hardware =>
                       algorithms.All(algorithm => file.HashRates[hardware].ContainsKey(algorithm)));
        }
    }
}
