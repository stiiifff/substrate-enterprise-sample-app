using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot;
using Polkadot.Api;
using Prism;
using Prism.Ioc;

namespace Parity.Substrate.EnterpriseSample.Droid
{
    [Activity(Label = "Parity.Substrate.EnterpriseSample", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App(new AndroidInitializer(Assets)));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
    
    public class AndroidInitializer : IPlatformInitializer
    {
        private readonly AssetManager assets;

        public AndroidInitializer(AssetManager assets)
        {
            this.assets = assets;
        }

        public void RegisterTypes(IContainerRegistry container)
        {
            //var logger = new Logger();
            //var jsonrpc = new JsonRpc(new Wsclient(logger), logger, new JsonRpcParams { JsonrpcVersion = "2.0" });
            container.RegisterInstance<IApplication>(PolkaApi.GetApplication());
            container.RegisterInstance<ILightClient>(new LightClient(assets));
        }
    }
}