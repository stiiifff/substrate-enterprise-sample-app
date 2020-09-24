using Android.Content.PM;
using Android.Content.Res;
using Android.Util;
using Java.IO;
using Java.Lang;
using Parity.Substrate.EnterpriseSample.Models;
using Prism.Events;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Process = Java.Lang.Process;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class LightClient : ILightClient
    {
        string nodeBasePath, nodeBinPath, nodeChainSpecPath;
        Process nodeProcess;
        readonly ReplaySubject<string> logs = new ReplaySubject<string>(50);

        public LightClient(AssetManager assets,
            ApplicationInfo applicationInfo,
            ToastService toast,
            IEventAggregator eventAggregator)
        {
            Assets = assets;
            ApplicationInfo = applicationInfo;
            Toast = toast;
            EventAggregator = eventAggregator;
        }

        public AssetManager Assets { get; }
        public ApplicationInfo ApplicationInfo { get; }
        public ToastService Toast { get; set; }
        public IEventAggregator EventAggregator { get; }
        public bool IsInitialized { get; private set; }

        public bool IsRunning => nodeProcess != null && nodeProcess.IsAlive;

        public IObservable<string> Logs => logs.AsObservable();

        public async Task InitAsync()
        {
            (nodeBasePath, nodeBinPath, nodeChainSpecPath) = await InstallNodeBinaryAsync();
            IsInitialized = true;
            EventAggregator.GetEvent<NodeStatusEvent>().Publish(NodeStatus.NodeInitialized);
        }

        public async Task StartAsync()
        {
            if (IsInitialized && !IsRunning)
                await Task.Run(StartNode);
        }

        public async Task StopAsync()
        {
            await Task.Run(StopNode);
        }

        public async Task PurgeAsync()
        {
            if (IsRunning)
            {
                await StopAsync();
            }

            await PurgeChainAsync();
            await StartAsync();
        }

        async Task<(string, string, string)> InstallNodeBinaryAsync()
        {
            Trace.WriteLine("Installing node ...");

            string basePath, binPath, chainSpecPath;
            try
            {
                var appPath = Xamarin.Essentials.FileSystem.AppDataDirectory;

                var nodeBinDir = Path.Combine(appPath, "node/bin");
                if (!Directory.Exists(nodeBinDir))
                    Directory.CreateDirectory(nodeBinDir);

                var nodeCfgDir = Path.Combine(appPath, "node/config");
                if (!Directory.Exists(nodeCfgDir))
                    Directory.CreateDirectory(nodeCfgDir);

                chainSpecPath = Path.Combine(nodeCfgDir, "chain-spec.json");
                if (!System.IO.File.Exists(chainSpecPath))
                {
                    using var input = Assets.Open("chain-spec.json");
                    using var file = System.IO.File.OpenWrite(chainSpecPath);
                    await input.CopyToAsync(file);
                }

                basePath = Path.Combine(appPath, "node");
                binPath = Path.Combine(ApplicationInfo.NativeLibraryDir, "enterprise-sample");

                var chmod = await RunCommandAsync($"/system/bin/chmod 744 {binPath}");
                if (!string.IsNullOrEmpty(chmod))
                    Log.Debug(GetType().Name, chmod);

                //var res = await RunCommandAsync($"{binPath} purge-chain -y -d {basePath} --chain={chainSpecPath}");
                //if (!string.IsNullOrEmpty(res))
                //    Log.Debug(GetType().Name, res);
            }
            catch (System.Exception ex)
            {
                Toast.ShowShortToast("Node installation failed.");
                EventAggregator.GetEvent<NodeStatusEvent>().Publish(NodeStatus.NodeError);
                Trace.WriteLine(ex);
                throw;
            }

            Trace.WriteLine("Node succesfully installed.");

            return (basePath, binPath, chainSpecPath);
        }

        void StartNode()
        {
            Trace.WriteLine("Node starting ...");

            // Light client
            nodeProcess = StartProcess($"{nodeBinPath} -d {nodeBasePath} --chain={nodeChainSpecPath} --light --no-prometheus --no-telemetry");
            // Full node - Should give the possibility to run either light or full node in the UI
            //nodeProcess = StartProcess($"{nodeBinPath} -d {nodeBasePath} --chain={nodeChainSpecPath} --no-prometheus --no-telemetry");

            _ = Task.Factory.StartNew(async () =>
            {
                using var input = new InputStreamReader(nodeProcess.ErrorStream);
                using var reader = new BufferedReader(input);

                bool ready = false;
                string line;
                while (!string.IsNullOrEmpty((line = reader.ReadLine())))
                {
                    if (!ready && line.Contains("idle", StringComparison.InvariantCultureIgnoreCase))
                    {
                        Toast.ShowShortToast("Node is ready.");
                        EventAggregator.GetEvent<NodeStatusEvent>().Publish(NodeStatus.NodeReady);
                        ready = true;
                    }
                    logs.OnNext(line);
                }
                reader.Close();
                await nodeProcess.WaitForAsync();
            }, TaskCreationOptions.LongRunning);
        }

        void StopNode()
        {
            if (nodeProcess != null)
            {

                try
                {
                    if (nodeProcess.IsAlive)
                        nodeProcess.Destroy();
                }
                catch (Java.Lang.Exception e)
                {
                    Log.Error(GetType().Name, e.ToString());
                }
                finally
                {
                    try { nodeProcess.Dispose(); }
                    catch { }
                    nodeProcess = null;
                }
            }
        }

        async Task PurgeChainAsync()
        {
            await RunCommandAsync($"{nodeBinPath} purge-chain -y -d {nodeBasePath} --chain={nodeChainSpecPath}");
        }

        private async Task<string> RunCommandAsync(string command)
        {
            try
            {
                var process = Runtime.GetRuntime().Exec(command);
                using var input = new InputStreamReader(process.InputStream);
                using var reader = new BufferedReader(input);

                string line;
                var output = new System.Text.StringBuilder();
                while (!string.IsNullOrEmpty(line = await reader.ReadLineAsync()))
                {
                    output.AppendLine(line);
                }
                reader.Close();
                await process.WaitForAsync();

                return output.ToString();
            }
            catch (Java.IO.IOException e)
            {
                throw new RuntimeException(e);
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException(e);
            }
        }

        private Process StartProcess(string command)
        {
            try
            {
                return Runtime.GetRuntime().Exec(command);
            }
            catch (Java.IO.IOException e)
            {
                throw new RuntimeException(e);
            }
            catch (InterruptedException e)
            {
                throw new RuntimeException(e);
            }
        }
    }
}