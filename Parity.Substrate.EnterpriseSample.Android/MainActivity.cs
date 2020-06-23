using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot;
using Polkadot.Api;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.Droid
{
    [Activity(Label = "Parity.Substrate.EnterpriseSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        ILightClient LightClient => DependencyService.Get<ILightClient>();
        IJsonRpc PolkadotApi => DependencyService.Get<IJsonRpc>();

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            var logger = new Logger();
            var jsonrpc = new JsonRpc(new Wsclient(logger), logger, new JsonRpcParams { JsonrpcVersion = "2.0" });
            DependencyService.RegisterSingleton<IJsonRpc>(jsonrpc);
            DependencyService.RegisterSingleton<ILightClient>(new LightClient(Assets));

            await LightClient.InitAsync();
            _ = LightClient.StartAsync();

            LoadApplication(new App());
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await LightClient.StartAsync();
        }

        protected override async void OnPause()
        {
            base.OnPause();
            PolkadotApi?.Disconnect();
            await LightClient.StopASync();
        }

        protected override async void OnDestroy()
        {
            base.OnDestroy();
            try
            {
                PolkadotApi?.Disconnect();
                await LightClient.StopASync();
            }
            finally
            {
                PolkadotApi?.Dispose();
            }
        }
        
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}