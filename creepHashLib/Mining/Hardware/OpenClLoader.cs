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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Common;

namespace MultiCryptoToolLib.Mining.Hardware
{
    public class OpenClLoader : ILoaderAsync<ISet<Hardware>>
    {
        public ISet<Hardware> Load(CancellationToken ctx)
        {
            var platforms = ProcessHelper
                .ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--platforms", ctx)
                .Select(i => i.Split(';')[1])
                .ToList();

            return StringListToHardwares(
                ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--devices",
                    ctx), platforms);
        }

        public async Task<ISet<Hardware>> LoadAsync(CancellationToken ctx)
        {
            var platforms =
                (await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}",
                        "--platforms", ctx), ctx))
                .Select(i => i.Split(';')[1])
                .ToList();

            return StringListToHardwares(
                await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/MultiMinerOpenCL{Filename.GetFileExtensionOs()}", "--devices",
                        ctx), ctx), platforms);
        }

        private static ISet<Hardware> StringListToHardwares(IEnumerable<string> strings, IList<string> platforms) =>
            new HashSet<Hardware>(strings.Select(i =>
            {
                var tokens = i.Split(';');
                var platformIndex = int.Parse(tokens[1]);

                return new Hardware
                {
                    Index = int.Parse(tokens[0]),
                    Name = tokens[2],
                    PlatformIndex = platformIndex,
                    Platform = platforms[platformIndex],
                    Type = HardwareType.OpenCl
                };
            }));
    }
}