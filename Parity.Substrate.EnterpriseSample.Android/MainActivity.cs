﻿using Android.App;
using Android.Content.PM;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism;
using Prism.Ioc;

namespace Parity.Substrate.EnterpriseSample.Droid
{
    [Activity(Label = "Substrate Enterprise Sample", Icon = "@mipmap/launcher_foreground", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);

            LoadApplication(new App(new AndroidInitializer(Assets, ApplicationInfo)));
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
        private readonly ApplicationInfo applicationInfo;

        public AndroidInitializer(AssetManager assets, ApplicationInfo applicationInfo)
        {
            this.assets = assets;
            this.applicationInfo = applicationInfo;
        }

        public void RegisterTypes(IContainerRegistry container)
        {
            container.RegisterInstance(assets);
            container.RegisterInstance(applicationInfo);
            container.RegisterInstance(PolkaApi.GetApplication());
            container.RegisterSingleton<IToastService, ToastService>();
            container.RegisterSingleton<ILightClient, LightClient>();
        }
    }
}