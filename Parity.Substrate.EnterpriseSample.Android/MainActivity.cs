using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.Droid
{
    [Activity(Label = "Parity.Substrate.EnterpriseSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        const string NodeUrl = "ws://127.0.0.1:9944";

        ILightClient LightClient => DependencyService.Get<ILightClient>();
        IApplication PolkadotApi => DependencyService.Get<IApplication>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            DependencyService.RegisterSingleton(PolkaApi.GetAppication());
            DependencyService.RegisterSingleton<ILightClient>(new LightClient(Assets));
            await LightClient.InitAsync();
            await LightClient.StartAsync();

            LoadApplication(new App());
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await LightClient.StartAsync();

            _ = Task.Run(async () => {
                try
                {
                    while (!LightClient.IsRunning)
                        await Task.Delay(TimeSpan.FromSeconds(1));
                    PolkadotApi?.Connect(NodeUrl);
                }
                catch (System.Exception e)
                {
                    Log.Debug("Polkadot", e.ToString());
                }
            });
        }

        protected override void OnPause()
        {
            base.OnPause();
            PolkadotApi?.Disconnect();
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            PolkadotApi?.Disconnect();
            await LightClient.StopASync();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}