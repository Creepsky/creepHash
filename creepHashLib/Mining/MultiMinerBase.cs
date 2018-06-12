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
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using creepHashLib.Benchmark.File;
using creepHashLib.Common;
using creepHashLib.Common.Logging;
using creepHashLib.Mining.Hardware;
using creepHashLib.Network;
using creepHashLib.Network.Proxy;

namespace creepHashLib.Mining
{
    using HardwareMinerProfit = IDictionary<Hardware.Hardware, MinerProfit>;

    public class MinerProfit
    {
        public Coin Key;
        public Miner Miner;
        public double Profit;
    }

    public class MultiMinerBase
    {
        private readonly CancellationToken _ctx;
        private IList<Hardware.Hardware> _hardware;
        private IList<Miner> _miner;
        private Uri _proxy;
        private IDictionary<Miner, IBenchmarkFile> _benchmarks;
        private readonly IList<MiningInstance> _miningInstances = new List<MiningInstance>();

        public event Action<IList<Miner>> MinerLoaded;
        public event Action<IList<Hardware.Hardware>> HardwareLoaded;
        public event Action<IDictionary<Miner, IBenchmarkFile>> BenchmarksLoaded;
        public event Action<Uri> ProxyFound;

        public Uri Uri { get; }
        public string Coin { get; }
        public string Address { get; }

        public MultiMinerBase(Uri serverUri, string coin, string address, CancellationToken ctx)
        {
            Uri = serverUri;
            Coin = coin;
            Address = address;
            _ctx = ctx;
        }

        public void Run()
        {
            try
            {
                var proxy = GetProxyAndPorts();

                proxy.ContinueWith(i =>
                {
                    if (i.Status != TaskStatus.RanToCompletion) return;
                    _proxy = i.Result.uri;
                    ProxyFound?.Invoke(_proxy);
                }, _ctx);

                proxy.Wait(_ctx);

                var hardware = LoadHardware();
                var miner = LoadMiner();
                var benchmarks = LoadBenchmarksAsync(miner, hardware, proxy);

                hardware.ContinueWith(i =>
                {
                    if (i.Status != TaskStatus.RanToCompletion) return;
                    _hardware = i.Result;
                    HardwareLoaded?.Invoke(_hardware);
                }, _ctx);

                miner.ContinueWith(i =>
                {
                    if (i.Status != TaskStatus.RanToCompletion) return;
                    _miner = i.Result;
                    MinerLoaded?.Invoke(_miner);
                }, _ctx);

                benchmarks.ContinueWith(i =>
                {
                    if (i.Status != TaskStatus.RanToCompletion) return;
                    _benchmarks = i.Result;
                    BenchmarksLoaded?.Invoke(_benchmarks);
                }, _ctx);

                while (!_ctx.IsCancellationRequested)
                {
                    try
                    {
                        var coinInfos = new CoinInfoLoader(new Uri(Uri, "mineableCoins")).LoadAsync(_ctx);
                        var bestAlgorithms = GetBestAlgorithmsAsync(hardware, benchmarks, coinInfos);

                        //bestAlgorithms.ContinueWith(i =>
                        //{
                        //}, _ctx);

                        RunMiningInstancesAsync(bestAlgorithms, proxy).Wait(_ctx);

                        _ctx.WaitHandle.WaitOne(TimeSpan.FromMinutes(1));
                    }
                    catch (OperationCanceledException e)
                    {
                        if (e.CancellationToken.IsCancellationRequested) continue;
                        Logger.Exception("The mining instance(s) stopped", e);
                        _ctx.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                    catch (Exception e)
                    {
                        Logger.Exception("Could not start the mining instances", e);
                        _ctx.WaitHandle.WaitOne(TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Exception("Could not load the core", e);
            }

            Task.WaitAll(_miningInstances
                .Where(i => !i.Task.IsCanceled && !i.Task.IsCompleted && !i.Task.IsFaulted)
                .Select(i => i.Task).ToArray());

            Logger.Debug("Stopped");
        }

        //private async Task<bool> Login()
        //{
        //    Console.Write("Username: ");
        //    var user = Console.ReadLine();
            
        //    if (_ctx.IsCancellationRequested)
        //        throw new OperationCanceledException();

        //    Console.Write("Password: ");
        //    var pass = Common.Console.ReadPassword();
        //    Console.WriteLine();

        //    return await _loginManager.Login(user, pass);
        //}

        private async Task<(Uri, TimeSpan)> GetFastestServer()
        {
            try
            {
                IList<Uri> uris;

                try
                {
                    var proxyListLoader = new ProxyUriLoader(new Uri(Uri, "network"));
                    uris = await proxyListLoader.LoadAsync(_ctx);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not get the server network", e);
                }

                var sb = new StringBuilder();
                sb.AppendLine("Available server:");

                foreach (var i in uris)
                    sb.AppendLine($"- {i}");

                Logger.Info(sb.ToString());

                try
                {
                    var fastest = await PingTest.GetBestPing(uris);
                    Logger.Info($"Fastest server: {fastest.Item1} -> {(int)fastest.Item2.TotalMilliseconds} ms");
                    return fastest;
                }
                catch (Exception e)
                {
                    throw new Exception("Error while doing the ping speed test", e);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not determine the fastest proxy", e);
            }
        }

        private async Task<(Uri uri, IDictionary<Coin, int> ports)> GetProxyAndPorts()
        {
            try
            {
                var uri = await GetFastestServer();

                try
                {
                    var portsUri = new Uri(uri.Item1, "/mineableCoins");
                    var ports = await new PortLoader(portsUri).LoadAsync(_ctx);
                    return (uri.Item1, ports);
                }
                catch (Exception e)
                {
                    throw new Exception("Error while loading the coin ports", e);
                }
            }
            catch (Exception e)
            {
                throw new Exception("Error while loading the network IPs", e);
            }
        }
        
        private Task<IList<Miner>> LoadMiner()
        {
            try
            {
                var miner = Miner.Miners;
                var loadedMiner = new List<Miner>();
                var sb = new StringBuilder("Found miner:");

                foreach (var m in miner)
                {
                    if (File.Exists(m.Path))
                    {
                        sb.AppendLine($"- {m}");
                        loadedMiner.Add(m);
                    }
                    else
                        Logger.Warning($"Could not load {m}");
                }

                Logger.Info(sb.ToString());

                _miner = loadedMiner;
                MinerLoaded?.Invoke(_miner);

                return Task.FromResult((IList<Miner>)loadedMiner);
            }
            catch (Exception e)
            {
                throw new Exception("Could not load the miner", e);
            }
        }

        private Task<IList<Hardware.Hardware>> LoadHardware()
        {
            var hardware = new List<Hardware.Hardware>();
            
            try
            {
                ISet<Hardware.Hardware> cudaDevices;
                ISet<Hardware.Hardware> openclDevices;

                try
                {
                    cudaDevices = new CudaLoader().Load(_ctx);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not load CUDA devices", e);
                }

                try
                {
                    openclDevices = new OpenClLoader().Load(_ctx);
                }
                catch (Exception e)
                {
                    throw new Exception("Could not load OpenCL devices", e);
                }

                hardware.AddRange(cudaDevices);
                hardware.AddRange(openclDevices);
            
                var sb = new StringBuilder();
                sb.AppendLine("Found hardware:");

                foreach (var h in hardware)
                    sb.AppendLine($"- {h}");

                Logger.Info(sb.ToString());

                return Task.FromResult((IList<Hardware.Hardware>)hardware);
            }
            catch (Exception e)
            {
                throw new Exception("Could not load the mining hardware", e);
            }
        }

        private async Task<IDictionary<Miner, IBenchmarkFile>> LoadBenchmarksAsync(Task<IList<Miner>> minerTask,
            Task<IList<Hardware.Hardware>> hardwareTask, Task<(Uri uri, IDictionary<Coin, int> ports)> proxyTask)
        {
            var miner = await minerTask;
            var hardware = await hardwareTask;
            return LoadBenchmarks(miner, hardware, proxyTask.Result.uri, proxyTask.Result.ports);
        }

        private Dictionary<Miner, IBenchmarkFile> LoadBenchmarks(IList<Miner> miner, IEnumerable<Hardware.Hardware> hardware, Uri uri, IDictionary<Coin, int> ports)
        {
            var benchmarkFiles = miner.ToDictionary(i => i,
                i => (IBenchmarkFile)new BenchmarkFileJson($"benchmarks/{i.Name}-{i.Version}.json"));

            var sb = new StringBuilder();
            sb.AppendLine("Loaded benchmark files:");

            foreach (var bf in benchmarkFiles)
            {
                bf.Value.Load(_ctx);
                sb.AppendLine($"- {bf.Key} with {bf.Value.HashRates.SelectMany(i => i.Value.Values).Count()} benchmarked algorithm(s)");
            }

            Logger.Info(sb.ToString());

            var identicalHardware = hardware.GroupBy(i => (i.PciBus, i.PciSlot)).ToList();
            var maxGroupSize = identicalHardware.Max(i => i.Count());

            for (var i = 0; i < maxGroupSize; ++i)
            {
                var hardwareGroup = identicalHardware.Where(j => j.Count() > i).Select(j => j.ElementAt(i)).ToList();

                foreach (var m in miner)
                {
                    var unfinishedAlgorithms = benchmarkFiles[m].GetUnfinishedAlgorithms(m, hardwareGroup).ToList();

                    if (!unfinishedAlgorithms.Any()) continue;

                    var threads = unfinishedAlgorithms
                        .GroupBy(j => j.hardware, j => j.algorithm)
                        .Select(g => new Thread(() =>
                        {
                            var h = g.Key;

                            foreach (var a in g)
                            {
                                Logger.Info($"Benchmarking {a} on {h} with {m}");
                                if (!benchmarkFiles[m].HashRates.ContainsKey(h)) benchmarkFiles[m].HashRates.Add(h, new Dictionary<string, HashRate>());
                                var result = Benchmark.Benchmark.Create(m, a, h, uri, ports).Load(_ctx);
                                benchmarkFiles[m].HashRates[h].Add(a, result);
                                Logger.Info($"Benchmarked {a} on {h} with {m}: {result}");

                                lock (benchmarkFiles[m])
                                {
                                    benchmarkFiles[m].Save();
                                }

                                if (_ctx.IsCancellationRequested) break;
                            }
                        }))
                        .ToList();

                    foreach (var thread in threads)
                        thread.Start();

                    foreach (var thread in threads)
                        thread.Join();
                }
            }

            foreach (var bf in benchmarkFiles)
                bf.Value.Save();

            return benchmarkFiles;
        }

        private static async Task<HardwareMinerProfit> GetBestAlgorithmsAsync(
            Task<IList<Hardware.Hardware>> hardwareTask,
            Task<IDictionary<Miner, IBenchmarkFile>> benchmarkFilesTask,
            Task<IList<Coin.Info>> coinInfosTask)
        {
            var hardware = await hardwareTask;
            var benchmarkFiles = await benchmarkFilesTask;
            var coinInfos = await coinInfosTask;
            return GetBestAlgorithms(hardware, benchmarkFiles, coinInfos);
        }

        private static HardwareMinerProfit GetBestAlgorithms(
            IEnumerable<Hardware.Hardware> hardware, IDictionary<Miner, IBenchmarkFile> benchmarkFiles,
            IEnumerable<Coin.Info> coinInfos)
        {
            Logger.Info("Calculating most profitable algorithms...");
            var bestHardwareAlgorithms = hardware.ToDictionary(i => i, i => default(MinerProfit));

            foreach (var c in coinInfos)
            {
                var a = c.Coin.Algorithm;

                foreach (var b in benchmarkFiles)
                {
                    foreach (var h in b.Value.HashRates)
                    {
                        if (!h.Value.ContainsKey(a)) continue;

                        var profit = c.Profitability * h.Value[a].Value;

                        if (!bestHardwareAlgorithms.ContainsKey(h.Key)) continue;

                        if (bestHardwareAlgorithms[h.Key] == null ||
                            bestHardwareAlgorithms[h.Key].Profit < profit)
                        {
                            bestHardwareAlgorithms[h.Key] = new MinerProfit
                            {
                                Key = c.Coin,
                                Miner = b.Key,
                                Profit = profit
                            };
                        }
                    }
                }
            }

            foreach (var i in bestHardwareAlgorithms.GroupBy(i => i.Key.PciSlot).ToList())
            {
                var ordered = i.OrderByDescending(j => j.Value.Profit).ToList();

                for (var j = 1; j < ordered.Count; ++j)
                    bestHardwareAlgorithms.Remove(ordered[j].Key);
            }

            var sb = new StringBuilder();
            sb.AppendLine("Most profitable algorithms:");

            foreach (var bha in bestHardwareAlgorithms)
                sb.AppendLine($"- {bha.Key}: {bha.Value?.Key}");

            Logger.Info(sb.ToString());

            return bestHardwareAlgorithms;
        }

        private async Task RunMiningInstancesAsync(
            Task<HardwareMinerProfit> bestAlgorithmsTask,
            Task<(Uri uri, IDictionary<Coin, int> ports)> proxyTask)
        {
            try
            {

                var bestAlgorithms = await bestAlgorithmsTask;
                var (uri, ports) = await proxyTask;

                if (bestAlgorithms.Values.All(i => i == null) || uri == null || !ports.Any())
                {
                    Logger.Warning("Nothing to mine...");
                    return;
                }

                foreach (var ba in bestAlgorithms)
                {
                    if (ba.Value == null)
                        continue;

                    var hardwareInUse = _miningInstances.FirstOrDefault(i => i.MiningHardware.Contains(ba.Key));

                    var alreadyRunning = _miningInstances.Any(i =>
                        hardwareInUse != null &&
                        i.Algorithm == ba.Value.Key.Algorithm &&
                        i.Miner == ba.Value.Miner);

                    if (!alreadyRunning)
                    {
                        if (hardwareInUse != null)
                        {
                            Logger.Info(
                                $"Stopping mining {hardwareInUse.Algorithm} with {hardwareInUse.Miner} on {hardwareInUse.MiningHardware.First()}");
                            hardwareInUse.Cancel();
                            hardwareInUse.Task?.Wait(_ctx);
                            _miningInstances.Remove(hardwareInUse);
                            Logger.Info(
                                $"Stopped mining {hardwareInUse.Algorithm} with {hardwareInUse.Miner} on {hardwareInUse.MiningHardware.First()}");
                        }

                        var miningInstance = MiningInstance.Create(
                            ba.Value.Miner,
                            new[] {ba.Key},
                            ba.Value.Key.Algorithm,
                            Coin, Address,
                            IPAddress.Parse(uri.Host), ports[ba.Value.Key]);

#pragma warning disable 4014
                        miningInstance.RunAsync(_ctx);
#pragma warning restore 4014

                        _miningInstances.Add(miningInstance);
                        Logger.Info($"Started mining {ba.Value.Key} with {ba.Value.Miner} on {ba.Key}");
                    }
                }
            }
            catch (Exception e)
            {
                throw new Exception("Could not run the mining instances", e);
            }
        }

        //private IEnumerable<MiningInstance> CreateMiningInstances(
        //    HardwareMinerAlgorithmProfit bestAlgorithms, IPAddress ip, IDictionary<Algorithm, int> ports)
        //{
        //    return bestAlgorithms.Values
        //        .Where(i => i != null)
        //        .Select(i => MiningInstance.Create(i.Miner,
        //            bestAlgorithms.Where(j => j.Value.Algorithm == i.Algorithm && j.Value.Miner == i.Miner)
        //                .Select(j => j.Key), i.Algorithm, ip, ports[i.Algorithm])).ToList();
        //}
    }
}
