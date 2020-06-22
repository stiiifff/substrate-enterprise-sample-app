using Android.Content.Res;
using Android.Util;
using Java.IO;
using Java.Lang;
using Parity.Substrate.EnterpriseSample;
using System.IO;
using System.Threading.Tasks;

[assembly: Xamarin.Forms.Dependency(typeof(LightClient))]

namespace Parity.Substrate.EnterpriseSample
{
    public class LightClient : ILightClient
    {
        string nodeBinPath, nodeDataDir;
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
            (nodeBinPath, nodeDataDir) = await InstallNodeBinaryAsync();
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

        async Task<(string, string)> InstallNodeBinaryAsync()
        {
            var appPath = Xamarin.Essentials.FileSystem.AppDataDirectory;

            var nodeBinDir = Path.Combine(appPath, "node/bin");
            if (!Directory.Exists(nodeBinDir))
                Directory.CreateDirectory(nodeBinDir);
            
            var nodeDataDir = Path.Combine(appPath, "node/data");
            if (!Directory.Exists(nodeDataDir))
                Directory.CreateDirectory(nodeDataDir);

            var nodeBinPath = Path.Combine(nodeBinDir, "io.parity.substrate.node-template");
            if (!System.IO.File.Exists(nodeBinPath))
            {
                using var input = Assets.Open("node-template");
                using var file = System.IO.File.OpenWrite(nodeBinPath);
                await input.CopyToAsync(file);
            }

            var chdmod = await RunCommandAsync($"/system/bin/chmod 744 {nodeBinPath}");
            if (!string.IsNullOrEmpty(chdmod))
                Log.Debug(GetType().Name, chdmod);

            var res = await RunCommandAsync($"{nodeBinPath} purge-chain --dev --light -d {nodeDataDir} -y");
            if (!string.IsNullOrEmpty(res))
                Log.Debug(GetType().Name, res);

            return (nodeBinPath, nodeDataDir);
        }

        void StartNode()
        {
            nodeProcess = StartProcess($"{nodeBinPath} --dev --light -d {nodeDataDir} --no-prometheus --no-telemetry");
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