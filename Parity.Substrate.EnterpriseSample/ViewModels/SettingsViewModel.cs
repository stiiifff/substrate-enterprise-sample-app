using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.Data;
using Prism.Navigation;
using Prism.Services;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class SettingsViewModel : TabViewModel
    {
        private IDisposable bestBlockSub;

        public SettingsViewModel(INavigationService navigationService,
            IDeviceService device, ILightClient lightClient,
            IApplication polkadotApi, INodeService node)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Settings";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
            Device = device;
            IsActiveChanged += OnIsActiveChanged;

            bestBlockSub = node.BestBlock.Subscribe(block =>
                Device.BeginInvokeOnMainThread(() =>
                    BestBlock = $"{block}"));
        }

        private async void OnIsActiveChanged(object sender, EventArgs e)
        {
            if (IsActive && App.IsPolkadotApiConnected)
                await LoadDataAsync();
            if (!IsActive)
                bestBlockSub?.Dispose();
        }

        private string applicationVersion;
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { SetProperty(ref applicationVersion, value); }
        }

        public IDeviceService Device { get; }

        private string bestBlock;
        public string BestBlock
        {
            get { return bestBlock; }
            set { SetProperty(ref bestBlock, value); }
        }

        private PeersInfo peersInfo;
        public PeersInfo PeersInfo
        {
            get { return peersInfo; }
            set { SetProperty(ref peersInfo, value); }
        }

        private SystemInfo systemInfo;
        public SystemInfo SystemInfo
        {
            get { return systemInfo; }
            set { SetProperty(ref systemInfo, value); }
        }

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var sys = GetSystemInfo();
                        var peers = GetSystemPeers();
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            SystemInfo = sys;
                            PeersInfo = peers;
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex.ToString());
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }

        public SystemInfo GetSystemInfo()
        {
            return PolkadotApi.GetSystemInfo();
        }

        public PeersInfo GetSystemPeers()
        {
            return PolkadotApi.GetSystemPeers();
        }
    }
}