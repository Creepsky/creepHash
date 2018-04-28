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
using System.IO;
using System.Threading;
using creepHashLib.Common;
using creepHashLib.Common.Logging;
using creepHashLib.Mining.Hardware;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace creepHashLib.Benchmark.File
{
    public class BenchmarkFileJson : IBenchmarkFile
    {
        public IDictionary<Hardware, IDictionary<string, HashRate>> HashRates { get; set; }
        public string Path { get; }

        public BenchmarkFileJson(string path)
        {
            Path = path;
        }

        public void Save()
        {
            var hashRatesJson = new JArray();

            foreach (var hardware in HashRates)
            {
                var algorithmsJson = new JObject();

                foreach (var hashRate in hardware.Value)
                    algorithmsJson.Add(hashRate.Key.ToJson(hashRate.Value));
                
                hashRatesJson.Add(new JObject
                {
                    new JProperty("device", hardware.Key.ToJson()),
                    new JProperty("algorithms", algorithmsJson)
                });
            }

            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            System.IO.File.WriteAllText(Path, hashRatesJson.ToString(Formatting.Indented));
        }

        public IDictionary<Hardware, IDictionary<string, HashRate>> Load(CancellationToken ctx)
        {
            HashRates = new Dictionary<Hardware, IDictionary<string, HashRate>>();
            
            if (System.IO.File.Exists(Path))
            {
                try
                {
                    var json = System.IO.File.ReadAllText(Path);

                    foreach (var hashRatesJson in JArray.Parse(json))
                    {
                        var hashRateJson = (JObject)hashRatesJson;
                        var hardwareJson = hashRateJson.Value<JObject>("device");
                        var hashRates = new Dictionary<string, HashRate>();

                        JsonHelper.FromJson(hardwareJson, out var hardware);

                        foreach (var algorithmJson in hashRateJson.Value<JObject>("algorithms").Children<JProperty>())
                        {
                            JsonHelper.FromJson(algorithmJson, out var algorithm, out var hashRate);
                            hashRates.Add(algorithm, hashRate);
                        }

                        HashRates.Add(hardware, hashRates);
                    }
                }
                catch (Exception e)
                {
                    Logger.Exception($"Could not load the benchmark file {Path}", e);
                }
            }

            return HashRates;
        }
    }
}