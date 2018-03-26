using System.Collections.Generic;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Mining.Hardware;

namespace MultiCryptoToolLib.Mining
{
    public class Miner
    {
        public string Name { get; }

        public string Version { get; }

        public string Path => $"miner/{Name}{Filename.GetFileExtensionOs()}";

        public ISet<Algorithm> Algorithms { get; }

        public ISet<HardwareType> HardwareTypes { get; }

        public Miner(string name, string version, ISet<HardwareType> hardwareTypes, ISet<Algorithm> algorithms)
        {
            Name = name;
            Version = version;
            Algorithms = algorithms;
            HardwareTypes = hardwareTypes;
        }

        public static readonly Miner CcMiner = new Miner("ccminer", "2.2.4",
            new HashSet<HardwareType>
            {
                HardwareType.Cuda
            },
            new HashSet<Algorithm>
            {
                Algorithm.Blakecoin,
                Algorithm.Blake,
                Algorithm.Blake2S,
                Algorithm.Bastion,
                Algorithm.Deep,
                Algorithm.Decred,
                Algorithm.Equihash,
                Algorithm.Fresh,
                Algorithm.Fugue256,
                Algorithm.Groestl,
                Algorithm.Hmq1725,
                Algorithm.Hsr,
                Algorithm.Keccak,
                Algorithm.Jackpot,
                Algorithm.Jha,
                Algorithm.Lbry,
                Algorithm.Luffa,
                Algorithm.Lyra2,
                Algorithm.Lyra2V2,
                Algorithm.Lyra2Z,
                Algorithm.MyrGr,
                Algorithm.Neoscrypt,
                Algorithm.Nist5,
                Algorithm.Penta,
                Algorithm.Phi,
                Algorithm.Polytimos,
                Algorithm.Qubit,
                Algorithm.Sha256D,
                Algorithm.Sha256T,
                Algorithm.Sia,
                Algorithm.Sib,
                Algorithm.Scrypt,
                Algorithm.Skein,
                Algorithm.Skein2,
                Algorithm.Skunk,
                Algorithm.S3,
                Algorithm.Timetravel,
                Algorithm.Tribus,
                Algorithm.Bitcore,
                Algorithm.X11Evo,
                Algorithm.X11,
                Algorithm.X13,
                Algorithm.X14,
                Algorithm.X15,
                Algorithm.X17,
                Algorithm.Vanilla,
                Algorithm.Veltor,
                Algorithm.Whirlpool,
                Algorithm.Zr5,
            });

        public static readonly Miner EthMiner = new Miner("ethminer", "0.14.0.dev2",
            new HashSet<HardwareType>
            {
                HardwareType.OpenCl,
                HardwareType.Cuda
            },
            new HashSet<Algorithm>
            {
                Algorithm.Ethash
            });

        public override string ToString() => $"{Name} v.{Version}";
    }
}