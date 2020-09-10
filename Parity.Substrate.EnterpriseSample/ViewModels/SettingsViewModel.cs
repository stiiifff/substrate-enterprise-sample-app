using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.Data;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class SettingsViewModel : TabViewModel
    {
        private IDisposable bestBlockSub;

        public SettingsViewModel(INavigationService navigationService,
            IDeviceService device, ILightClient lightClient,  IApplication polkadotApi,
            INodeService node, IToastService toast, IPageDialogService pageDialog)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Settings";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
            Device = device;
            Toast = toast;
            PageDialog = pageDialog;

            PurgeNodeDataCommand = new Command(async () => await PurgeNodeDataAsync());
            
            bestBlockSub = node.BestBlock.Subscribe(block =>
                Device.BeginInvokeOnMainThread(() =>
                    BestBlock = $"{block}"));

            IsActiveChanged += OnIsActiveChanged;
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }
        public IPageDialogService PageDialog { get; }
        public ICommand PurgeNodeDataCommand { get; }

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

        private SystemInfo GetSystemInfo()
        {
            return PolkadotApi.GetSystemInfo();
        }

        private PeersInfo GetSystemPeers()
        {
            return PolkadotApi.GetSystemPeers();
        }

        private async Task PurgeNodeDataAsync()
        {
            try
            {
                if (await PageDialog.DisplayAlertAsync("Purge node data", "Please confirm you want to purge the nodes's data.", "OK", "Cancel"))
                    await LightClient.PurgeAsync();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
                Toast.ShowShortToast($"Error purging data.");
            }
        }
    }
}