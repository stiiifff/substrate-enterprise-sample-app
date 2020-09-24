using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Parity.Substrate.EnterpriseSample.ViewModels;
using Parity.Substrate.EnterpriseSample.Views;
using Plugin.Iconize;
using Polkadot.Api;
using Prism;
using Prism.Events;
using Prism.Ioc;
using Xamarin.Essentials;
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

        protected override void RegisterTypes(IContainerRegistry container)
        {
            container.RegisterSingleton<IAppInfo, AppInfoImplementation>();
            container.RegisterSingleton<IAccountService, AccountService>();
            container.RegisterSingleton<INodeService, NodeService>();

            container.RegisterForNavigation<NavigationPage>();
            container.RegisterForNavigation<IconNavigationPage>();
            container.RegisterForNavigation<AccountCreationPage, AccountCreationViewModel>();
            container.RegisterForNavigation<AccountInfoPage, AccountInfoViewModel>();
            container.RegisterForNavigation<AccountMnemonicPage, AccountMnemonicViewModel>();
            container.RegisterForNavigation<MainPage, MainViewModel>();
            container.RegisterForNavigation<TrackingPage, TrackingViewModel>();
            container.RegisterForNavigation<ManagePage, ManageViewModel>();
            container.RegisterForNavigation<ShipmentPage, ShipmentViewModel>();
            container.RegisterForNavigation<ShipmentJourneyPage, ShipmentJourneyViewModel>();
            container.RegisterForNavigation<SettingsPage, SettingsViewModel>();
            container.RegisterForNavigation<NodeLogsPage, NodeLogsViewModel>();
            container.RegisterForNavigation<SignExternalPage, SignExternalPageViewModel>();
        }

        protected override async void OnInitialized()
        {
            InitializeComponent();
            SubscribeToNodeEvents();
            _ = Task.Run(() => LightClient.InitAsync());

            await NavigationService.NavigateAsync(
                Preferences.Get("IsFirstRun", true)
                ? "/IconNavigationPage/AccountCreationPage"
                : "/IconNavigationPage/MainPage"
             );
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
                await LightClient.StopAsync();
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
                        await Task.Delay(1000);
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
