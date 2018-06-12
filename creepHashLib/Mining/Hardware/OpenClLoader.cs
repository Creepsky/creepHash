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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Common;

namespace creepHashLib.Mining.Hardware
{
    public class OpenClLoader : ILoaderAsync<ISet<Hardware>>
    {
        public ISet<Hardware> Load(CancellationToken ctx)
        {
            var platforms = ProcessHelper
                .ReadLines($"utils/creepHashOpenCL{Filename.GetFileExtensionOs()}", "--platforms", ctx)
                .Select(i => i.Split(';'))
                .Where(i => i.Length >= 2)
                .Select(i => i[1])
                .ToList();

            var devices = ProcessHelper
                .ReadLines($"utils/creepHashOpenCL{Filename.GetFileExtensionOs()}", "--devices", ctx);

            return StringListToHardwares(devices, platforms);
        }

        public async Task<ISet<Hardware>> LoadAsync(CancellationToken ctx)
        {
            var platforms =
                (await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/creepHashOpenCL{Filename.GetFileExtensionOs()}",
                        "--platforms", ctx), ctx))
                .Select(i => i.Split(';'))
                .Where(i => i.Length >= 1)
                .Select(i => i[1])
                .ToList();

            return StringListToHardwares(
                await Task.Run(
                    () => ProcessHelper.ReadLines($"utils/creepHashOpenCL{Filename.GetFileExtensionOs()}", "--devices",
                        ctx), ctx), platforms);
        }

        private static ISet<Hardware> StringListToHardwares(IEnumerable<string> strings, IList<string> platforms)
        {
            var hardwares = strings.Select(i =>
                {
                    var tokens = i.Split(';');

                    if (tokens.Length < 5)
                    {
                        //throw new Exception($"Not enough tokens for mining hardware in '{tokens}'");
                        return null;
                    }

                    var platformIndex = int.Parse(tokens[1]);

                    if (platformIndex >= platforms.Count)
                        throw new Exception(
                            $"Platform index for mining hardware {tokens[2]} is {platformIndex}, but there are only {platforms.Count} platforms available!");

                    return new Hardware(HardwareType.OpenCl, int.Parse(tokens[0]), platformIndex, tokens[2],
                        platforms[platformIndex], int.Parse(tokens[3]), int.Parse(tokens[4]));
                })
                .Where(i => i != null)
                .ToList();

            return new HashSet<Hardware>(hardwares);
        }
    }
}
