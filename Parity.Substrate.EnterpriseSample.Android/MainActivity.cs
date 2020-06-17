
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using Java.IO;
using Android.Util;
using System.IO;
using System.Threading.Tasks;
using Java.Lang;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.Droid
{
    [Activity(Label = "Parity.Substrate.EnterpriseSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        string nodeBinPath, nodeDataDir;
        Java.Lang.Process nodeProcess;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            DependencyService.RegisterSingleton<ILightClient>(new LightClient());

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            //(nodeBinPath, nodeDataDir) = await InstallNodeBinaryAsync();

            LoadApplication(new App());
        }

        protected override void OnResume()
        {
            base.OnResume();

            //nodeProcess = StartProcess($"{nodeBinPath} --dev --light -d {nodeDataDir}");
        }

        protected override void OnPause()
        {
            base.OnPause();
            //if (nodeProcess != null && nodeProcess.IsAlive)
            //{
            //    try
            //    {
            //        nodeProcess.Destroy();
            //    }
            //    catch (Java.Lang.Exception e)
            //    {
            //        Log.Error("Substrate", e.ToString());
            //    }
            //    finally
            //    {
            //        nodeProcess.Dispose();
            //    }
            //}
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        async Task<(string, string)> InstallNodeBinaryAsync()
        {
            var appPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
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

                var chdmod = RunCommandToCompletion($"/system/bin/chmod 744 {nodeBinPath}");
                if (!string.IsNullOrEmpty(chdmod))
                    Log.Debug("LightClient", chdmod);
            }

            var res = RunCommandToCompletion($"{nodeBinPath} purge-chain --dev --light -d {nodeDataDir} -y");
            if (!string.IsNullOrEmpty(res))
                Log.Debug("LightClient", res);
            
            return (nodeBinPath, nodeDataDir);
        }

        private string RunCommandToCompletion(string command)
        {
            try
            {
                var process = Runtime.GetRuntime().Exec(command);

                using var input = new InputStreamReader(process.InputStream);
                using var reader = new BufferedReader(input);

                int read;
                char[] buffer = new char[4096];
                var output = new StringBuffer();
                while ((read = reader.Read(buffer)) > 0)
                {
                    output.Append(buffer, 0, read);
                }
                reader.Close();
                process.WaitFor();

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

        private Java.Lang.Process StartProcess(string command)
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