using Prism;
using Prism.Ioc;
using Polkadot.Api;
using System.Diagnostics;
using Xamarin.Forms;
using Xamarin.Essentials.Interfaces;
using Xamarin.Essentials.Implementation;
using Parity.Substrate.EnterpriseSample.Views;
using Parity.Substrate.EnterpriseSample.Services;
using Parity.Substrate.EnterpriseSample.ViewModels;
using System.Threading.Tasks;
using System;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App
    {
        const string NodeUrl = "ws://127.0.0.1:9944";

        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        public bool IsPolkadotApiConnected { get; private set; }
        public IApplication PolkadotApi => Container.Resolve<IApplication>();
        public ILightClient LightClient => Container.Resolve<ILightClient>();

        internal bool ConnectToNode()
        {
            if (IsPolkadotApiConnected)
                return true;

            try
            {
                var _res = PolkadotApi.Connect(NodeUrl);
                return (IsPolkadotApiConnected = true);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();

            containerRegistry.RegisterForNavigation<NavigationPage>();
            containerRegistry.RegisterForNavigation<MainPage, MainViewModel>();
            containerRegistry.RegisterForNavigation<TrackingPage, TrackingViewModel>();
            containerRegistry.RegisterForNavigation<ManagePage, ManageViewModel>();
            containerRegistry.RegisterForNavigation<ShipmentPage, ShipmentViewModel>();
            containerRegistry.RegisterForNavigation<SettingsPage, SettingsViewModel>();
            containerRegistry.RegisterForNavigation<NodeLogsPage, NodeLogsViewModel>();
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();

            _ = LightClient.InitAsync()
                .ContinueWith(async _ => await Task.Delay(TimeSpan.FromSeconds(5)))
                .ContinueWith(async _ => await LightClient.StartAsync());

            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }

        protected override async void CleanUp()
        {
            base.CleanUp();
            try
            {
                if (IsPolkadotApiConnected)
                    PolkadotApi?.Disconnect();
                await LightClient.StopASync();
            }
            finally
            {
                PolkadotApi?.Dispose();
            }
        }

        protected override async void OnResume()
        {
            base.OnResume();
            await LightClient.StartAsync();
        }

        protected override async void OnSleep()
        {
            base.OnSleep();
            try
            {
                if (IsPolkadotApiConnected)
                    PolkadotApi?.Disconnect();
                await LightClient.StopASync();
            }
            finally
            {
                IsPolkadotApiConnected = false;
            }
        }
    }
}
