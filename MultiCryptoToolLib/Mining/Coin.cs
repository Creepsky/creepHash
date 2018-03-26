using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiCryptoToolLib.Mining
{
    public class Coin
    {
        public struct Info
        {
            public Coin Coin;
            public int Port;
            public double Profitability;
        }

        /// <summary>
        /// The name of the coin
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The short name of the coin
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// The hash algorithm that is used to mine the coin
        /// </summary>
        public Algorithm Algorithm { get; }

        public Coin(string name, string shortName, Algorithm algorithm)
        {
            Name = name;
            ShortName = shortName;
            Algorithm = algorithm;
        }

        public Info CreateInfo(int port, double profitability) =>
            new Info {Coin = this, Port = port, Profitability = profitability};

        public bool IsMinableWith(Miner miner) => miner.Algorithms.Contains(Algorithm);

        public static readonly Coin EthereumFuture = new Coin("Ethereum Future", "ETHF", Algorithm.Nist5);
        public static readonly Coin MonaCoin = new Coin("MonaCoin", "MONA", Algorithm.Lyra2V2);
        public static readonly Coin VertCoin = new Coin("VertCoin", "VTC", Algorithm.Lyra2V2);
        public static readonly Coin LiteCoinTest = new Coin("LiteCoin Test", "LTCT", Algorithm.Scrypt);

        public static readonly IList<Coin> AllCoins = new List<Coin>
        {
            EthereumFuture,
            MonaCoin,
            VertCoin,
            LiteCoinTest
        };

        public static Coin FromString(string name)
        {
            var coin = AllCoins.FirstOrDefault(i => string.Equals(i.Name, name, StringComparison.CurrentCultureIgnoreCase) ||
                                                    string.Equals(i.ShortName, name, StringComparison.CurrentCultureIgnoreCase));

            if (coin == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Unknown coin {name}");

            return coin;
        }

        public override string ToString()
        {
            return $"{Name} ({ShortName})";
        }
    }

    public static class CoinExtensions
    {
        public static bool IsMinableWith(this Coin coin, Miner miner)
        {
            if (coin == null)
                throw new ArgumentNullException(nameof(coin));

            if (miner == null)
                throw new ArgumentNullException(nameof(miner));

            return miner.Algorithms.Contains(coin.Algorithm);
        }
    }
}
