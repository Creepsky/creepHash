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
using System.Linq;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Mining.Hardware;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Mining
{
    public class Miner
    {
        private static readonly IList<Miner> Miners = new List<Miner>();

        public string Name { get; }

        public string Version { get; }

        public string Path => $"miner/{Name}{Filename.GetFileExtensionOs()}";

        public ISet<string> Algorithms { get; }

        public ISet<HardwareType> HardwareTypes { get; }

        public Miner(string name, string version, ISet<HardwareType> hardwareTypes, ISet<string> algorithms)
        {
            Name = name;
            Version = version;
            Algorithms = algorithms;
            HardwareTypes = hardwareTypes;
        }

        public override string ToString() => $"{Name} v.{Version}";

        public static void LoadFromJson(string path)
        {
            Miners.Clear();
            var lines = File.ReadAllText(path);
            var minersJson = JObject.Parse(lines);

            foreach (var token in minersJson)
            {
                var name = token.Key;
                var version = token.Value["version"].Value<string>();
                var hardware = new HashSet<HardwareType>(token.Value["hardware"].Values<string>().Select(i =>
                {
                    switch (i)
                    {
                        case "cpu":
                            return HardwareType.Cpu;
                        case "cuda":
                            return HardwareType.Cuda;
                        case "opencl":
                            return HardwareType.OpenCl;
                        default:
                            throw new KeyNotFoundException($"Invalid hardware {i} for miner {name}");
                    }
                }));
                var algorithms = new HashSet<string>(token.Value["algorithms"].Values<string>());

                Miners.Add(new Miner(name, version, hardware, algorithms));
            }
        }

        public static Miner FromString(string name)
        {
            var miner = Miners?.FirstOrDefault(i => i.Name == name);

            if (miner == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown miner {name}");

            return miner;
        } 
    }
}