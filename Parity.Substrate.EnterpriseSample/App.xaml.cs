using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Parity.Substrate.EnterpriseSample.ViewModels;
using Parity.Substrate.EnterpriseSample.Views;
using Polkadot.Api;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Xamarin.Essentials.Implementation;
using Xamarin.Essentials.Interfaces;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Parity.Substrate.EnterpriseSample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class App
    {
        const string NodeUrl = "ws://127.0.0.1:9944";

        SubscriptionToken nodeEventSubs;

        public App() : this(null) { }

        public App(IPlatformInitializer initializer) : base(initializer) { }

        public bool IsPolkadotApiConnected { get; private set; }
        public IApplication PolkadotApi => Container.Resolve<IApplication>();
        public IEventAggregator EventAggregator => Container.Resolve<IEventAggregator>();
        public ILightClient LightClient => Container.Resolve<ILightClient>();
        public IToastService ToastService => Container.Resolve<IToastService>();

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            containerRegistry.RegisterSingleton<INodeService, NodeService>();

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
            SubscribeToNodeEvents();
            _ = Task.Run(() => LightClient.InitAsync());
            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }

        protected override async void OnResume()
        {
            base.OnResume();
            SubscribeToNodeEvents();
            await StartNodeAsync();
        }

        protected override async void OnSleep()
        {
            base.OnSleep();
            UnsubscribeFromNodeEvents();
            await StopNodeAsync();
        }

        protected override async void CleanUp()
        {
            base.CleanUp();
            await StopNodeAsync();
        }

        internal async Task StartNodeAsync()
        {
            await LightClient.StartAsync();
        }

        internal async Task StopNodeAsync()
        {
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

        internal bool ConnectToNode()
        {
            if (IsPolkadotApiConnected)
                return true;

            try
            {
                IsPolkadotApiConnected = PolkadotApi.Connect(NodeUrl) == 0;
                if (IsPolkadotApiConnected)
                    EventAggregator.GetEvent<ApiStatusEvent>().Publish(ApiStatus.ApiReady);
                return IsPolkadotApiConnected;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                return false;
            }
        }

        internal void SubscribeToNodeEvents()
        {
            if (nodeEventSubs != null)
                return;

            nodeEventSubs = EventAggregator.GetEvent<NodeStatusEvent>().Subscribe(async status =>
            {
                switch (status)
                {
                    case NodeStatus.NodeInitialized:
                        await StartNodeAsync();
                        break;
                    case NodeStatus.NodeReady:
                        await Task.Delay(2000);
                        ConnectToNode();
                        break;
                    case NodeStatus.NodeError:
                        await StopNodeAsync();
                        break;
                }
            });
        }

        internal void UnsubscribeFromNodeEvents()
        {
            if (nodeEventSubs != null)
                EventAggregator.GetEvent<NodeStatusEvent>().Unsubscribe(nodeEventSubs);
        }
    }
}
