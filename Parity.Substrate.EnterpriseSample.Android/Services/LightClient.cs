using Android.Content.Res;
using Android.Util;
using Java.IO;
using Java.Lang;
using Parity.Substrate.EnterpriseSample.Services;
using System.IO;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(LightClient))]

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class LightClient : ILightClient
    {
        string nodeBasePath, nodeBinPath, nodeChainSpecPath;
        Process nodeProcess;

        public LightClient(AssetManager assets)
        {
            Assets = assets;
        }

        public AssetManager Assets { get; }

        public bool IsInitialized { get; private set; }

        public bool IsRunning => nodeProcess != null && nodeProcess.IsAlive;

        public async Task InitAsync()
        {
            (nodeBasePath, nodeBinPath, nodeChainSpecPath) = await InstallNodeBinaryAsync();
            IsInitialized = true;
        }

        public async Task StartAsync()
        {
            if (IsInitialized && !IsRunning)
                await Task.Run(StartNode);
        }

        public async Task StopASync()
        {
            await Task.Run(StopNode);
        }

        async Task<(string, string, string)> InstallNodeBinaryAsync()
        {
            var appPath = Xamarin.Essentials.FileSystem.AppDataDirectory;

            var nodeBinDir = Path.Combine(appPath, "node/bin");
            if (!Directory.Exists(nodeBinDir))
                Directory.CreateDirectory(nodeBinDir);

            var nodeCfgDir = Path.Combine(appPath, "node/config");
            if (!Directory.Exists(nodeCfgDir))
                Directory.CreateDirectory(nodeCfgDir);

            var nodeBasePath = Path.Combine(appPath, "node");

            var nodeBinPath = Path.Combine(nodeBinDir, "io.parity.substrate.node-template");
            if (!System.IO.File.Exists(nodeBinPath))
            {
                using var input = Assets.Open("node-template");
                using var file = System.IO.File.OpenWrite(nodeBinPath);
                await input.CopyToAsync(file);
            }

            var nodeChainSpecPath = Path.Combine(nodeCfgDir, "chain-spec.json");
            if (!System.IO.File.Exists(nodeChainSpecPath))
            {
                using var input = Assets.Open("chain-spec.json");
                using var file = System.IO.File.OpenWrite(nodeChainSpecPath);
                await input.CopyToAsync(file);
            }

            var chmod = await RunCommandAsync($"/system/bin/chmod 744 {nodeBinPath}");
            if (!string.IsNullOrEmpty(chmod))
                Log.Debug(GetType().Name, chmod);

            var res = await RunCommandAsync($"{nodeBinPath} purge-chain -y -d {nodeBasePath} --chain={nodeChainSpecPath}");
            if (!string.IsNullOrEmpty(res))
                Log.Debug(GetType().Name, res);

            return (nodeBasePath, nodeBinPath, nodeChainSpecPath);
        }

        void StartNode()
        {
            nodeProcess = StartProcess($"{nodeBinPath} -d {nodeBasePath} --chain={nodeChainSpecPath} --light --no-prometheus --no-telemetry");

            //using var input = new InputStreamReader(nodeProcess.ErrorStream);
            //using var reader = new BufferedReader(input);

            //string line;
            //var output = new System.Text.StringBuilder();
            //while (!string.IsNullOrEmpty((line = reader.ReadLine())))
            //{
            //    output.AppendLine(line);
            //}
            //reader.Close();
            //nodeProcess.WaitFor();
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

        private async Task<string> RunCommandAsync(string command)
        {
            try
            {
                var process = Runtime.GetRuntime().Exec(command);

                using var input = new InputStreamReader(process.InputStream);
                using var reader = new BufferedReader(input);

                string line;
                var output = new System.Text.StringBuilder();
                while (!string.IsNullOrEmpty((line = await reader.ReadLineAsync())))
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