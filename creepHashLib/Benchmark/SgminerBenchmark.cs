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
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Common;
using creepHashLib.Common.Logging;
using creepHashLib.Mining;
using creepHashLib.Mining.Hardware;

namespace creepHashLib.Benchmark
{
    public class SgminerBenchmark : Benchmark
    {
        private readonly Uri _uri;
        private readonly IDictionary<Coin, int> _ports;

        public SgminerBenchmark(string algorithm, Hardware miningHardware, Uri uri, IDictionary<Coin, int> ports)
            : base(Miner.FromString("sgminer"), algorithm, miningHardware)
        {
            _uri = uri;
            _ports = ports;
        }

        protected override HashRate Run(CancellationToken cancel)
        {
            if (cancel.IsCancellationRequested)
                cancel.ThrowIfCancellationRequested();


            foreach (var i in _ports)
            {
                if (i.Key.Algorithm != Algorithm) continue;

                var lines = ProcessHelper.ReadLines(
                    Miner.Path,
                    $"-o {_uri}:{i.Value} --algorithm={Algorithm} --device={MiningHardware.Index} -u {i.Key.ShortName.ToUpper()}:BENCHMARK --shares 1 --text-only -p x",
                    cancel
                ).ToList();

                foreach (var l in lines)
                {
                    var match = Regex.Match(l, @"");

                }
            }
            
            return new HashRate(0, Metric.Unit);
        }

        protected override Task<HashRate> RunAsync(CancellationToken cancel)
        {
            return Task.FromResult(Run(cancel));
        }
    }
}
