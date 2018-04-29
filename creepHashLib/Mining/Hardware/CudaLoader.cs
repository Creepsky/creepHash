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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Common;
using creepHashLib.Common.Logging;

namespace creepHashLib.Mining.Hardware
{
    public class CudaLoader : ILoaderAsync<ISet<Hardware>>
    {
        public ISet<Hardware> Load(CancellationToken cancellationToken) => StringListToHardwares(
            ProcessHelper.ReadLines($"miner/ethminer{Filename.GetFileExtensionOs()}", "-U --list-devices",
                cancellationToken));

        public async Task<ISet<Hardware>> LoadAsync(CancellationToken ctx) => StringListToHardwares(await Task.Run(
            () => ProcessHelper.ReadLines($"miner/ethminer{Filename.GetFileExtensionOs()}", "-U --list-devices",
                ctx), ctx));

        private static ISet<Hardware> StringListToHardwares(IEnumerable<string> strings)
        {
            var hardware = new HashSet<Hardware>();
            int? currentIndex = null;
            string currentName = null;

            foreach (var line in strings)
            {
                var matchHardware = Regex.Match(line, @"\[(\d+)\]\s(.+)");

                if (matchHardware.Success)
                {
                    currentIndex = int.Parse(matchHardware.Groups[1].Value);
                    currentName = matchHardware.Groups[2].Value;
                    continue;
                }

                var matchPci = Regex.Match(line, @".*Pci:.\s*[0-9A-Fa-f]{4}:(\d*):(\d*)");

                if (matchPci.Success)
                {
                    var pciBus = int.Parse(matchPci.Groups[1].Value);
                    var pciSlot = int.Parse(matchPci.Groups[2].Value);

                    if (currentIndex == null || string.IsNullOrEmpty(currentName))
                    {
                        Logger.Error(
                            $"Could not load a CUDA device at PCI BUS {pciBus}, slot {pciSlot}: name or index is empty");
                        continue;
                    }

                    hardware.Add(new Hardware(HardwareType.Cuda, currentIndex.Value, 0, currentName, null, pciBus,
                        pciSlot));

                    currentIndex = null;
                    currentName = null;
                }
            }

            return hardware;
        }
    }
}
