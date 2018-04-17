using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MultiCryptoToolLib.Mining.Hardware;
using Newtonsoft.Json.Linq;

namespace MultiCryptoToolLib.Mining
{
    public class Algorithm
    {
        private static readonly IList<Algorithm> Algorithms = new List<Algorithm>();

        public string Name { get; }

        public Algorithm(string name)
        {
            Name = name;
        }

        public bool IsMinableWith(Miner miner) => miner.Algorithms.Contains(this);

        public override string ToString()
        {
            return Name;
        }

        public static void LoadFromJson(string path)
        {
            Algorithms.Clear();
            var lines = File.ReadAllText(path);
            var algorithmsJson = JArray.Parse(lines);

            foreach (var token in algorithmsJson)
                Algorithms.Add(new Algorithm(token.ToString()));
        }

        public static Algorithm FromString(string name)
        {
            var algorithm = Algorithms.FirstOrDefault(i => i.Name == name.ToLower());

            if (algorithm == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown algorithm {name}");

            return algorithm;
        }
    }
}
