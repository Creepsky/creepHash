using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MultiCryptoToolLib.Benchmark.File;
using MultiCryptoToolLib.Common;
using MultiCryptoToolLib.Common.Logging;
using MultiCryptoToolLib.Mining.Hardware;
using MultiCryptoToolLib.Network;
using MultiCryptoToolLib.Network.Proxy;

namespace MultiCryptoToolLib.Mining
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
                var hardware = LoadHardware();
                var miner = LoadMiner();
                var benchmarks = LoadBenchmarksAsync(miner, hardware);
                var proxy = GetProxyAndPorts();

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

                proxy.ContinueWith(i =>
                {
                    if (i.Status != TaskStatus.RanToCompletion) return;
                    _proxy = i.Result.uri;
                    ProxyFound?.Invoke(_proxy);
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
                var miner = new List<Miner>
                {
                    Miner.FromString("ccminer"),
                    Miner.FromString("ethminer")
                };

                var loadedMiner = new List<Miner>();

                var sb = new StringBuilder();
                sb.AppendLine("Found miner:");

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
            var cudaGpus = new CudaLoader().LoadAsync(_ctx);
            var openclGpus = new OpenClLoader().LoadAsync(_ctx);
            
            try
            {
                ISet<Hardware.Hardware> cudaDevices;
                ISet<Hardware.Hardware> openclDevices;

                try
                {
                    cudaDevices = cudaGpus.Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Could not load CUDA devices", e);
                }

                try
                {
                    openclDevices = openclGpus.Result;
                }
                catch (Exception e)
                {
                    throw new Exception("Could not load OpenCL devices", e);
                }

                hardware.AddRange(cudaDevices.Any() ? cudaDevices : openclDevices);
                //hardware.AddRange(cudaGpus.Result);
                //hardware.AddRange(openclGpus.Result);
            
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
            Task<IList<Hardware.Hardware>> hardwareTask)
        {
            var miner = await minerTask;
            var hardware = await hardwareTask;
            return LoadBenchmarks(miner, hardware);
        }

        private Dictionary<Miner, IBenchmarkFile> LoadBenchmarks(IList<Miner> miner, IList<Hardware.Hardware> hardware)
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

            foreach (var m in miner)
            foreach (var (a, h) in benchmarkFiles[m].GetUnfinishedAlgorithms(m, hardware))
            {
                Logger.Info($"Benchmarking {a} on {h} with {m}");
                if (!benchmarkFiles[m].HashRates.ContainsKey(h))
                    benchmarkFiles[m].HashRates.Add(h, new Dictionary<string, HashRate>());
                var result = Benchmark.Benchmark.Create(m, a, h).Load(_ctx);
                benchmarkFiles[m].HashRates[h].Add(a, result);
                Logger.Info($"Benchmarked {a} on {h} with {m}: {result}");
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
            //var algorithmInfos = coinInfos.GroupBy(i => i.Coin.Algorithm).ToDictionary(i => i.Key, i => i.ToList());

            //Coin.Info GetBestCoin(IList<Coin.Info> ci)
            //{
            //    var best = new Coin.Info();

            //    for (var i = 0; i < ci.Count; ++i)
            //        if (i == 0 || best.Profitability < ci[i].Profitability)
            //            best = ci[i];

            //    return best;
            //}

            //var algorithmBestCoins = algorithmInfos.ToDictionary(i => i.Key, i => GetBestCoin(i.Value));

            //foreach (var a in algorithmBestCoins)
            foreach (var c in coinInfos)
            {
                var a = c.Coin.Algorithm;

                foreach (var b in benchmarkFiles)
                {
                    foreach (var h in b.Value.HashRates)
                    {
                        if (h.Value.ContainsKey(a))
                        {
                            var profit = c.Profitability * h.Value[a].Value;

                            if (!bestHardwareAlgorithms.ContainsKey(h.Key))
                                continue;

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
                        // TODO: changed from i.Algorithm == ba.Value.Key
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
