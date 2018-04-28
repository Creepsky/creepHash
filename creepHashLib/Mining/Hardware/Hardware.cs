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