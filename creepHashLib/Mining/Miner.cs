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