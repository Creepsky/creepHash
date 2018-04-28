using System.Collections.Generic;

namespace MultiCryptoToolLib.Mining.Hardware
{
    public enum HardwareType
    {
        Cpu,
        OpenCl,
        Cuda
    }

    public class Hardware
    {
        public HardwareType Type;
        public int Index;
        public int PlatformIndex;
        public string Name;
        public string Platform;

        public bool IsCompatibleWith(Miner miner) => miner.HardwareTypes.Contains(Type);

        public override bool Equals(object obj)
        {
            if (!(obj is Hardware))
                return false;

            var hardware = (Hardware)obj;
            return Type == hardware.Type &&
                   Index == hardware.Index &&
                   PlatformIndex == hardware.PlatformIndex &&
                   Name == hardware.Name &&
                   Platform == hardware.Platform;
        }

        public override int GetHashCode()
        {
            var hashCode = -520009746;
            //hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + Type.GetHashCode();
            hashCode = hashCode * -1521134295 + Index.GetHashCode();
            hashCode = hashCode * -1521134295 + PlatformIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Platform);
            return hashCode;
        }

        public override string ToString()
        {
            return $"{Name}, {Type}";
        }
    }
}