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
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel(INavigationService navigationService, IDeviceService device, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "About";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
            Device = device;
        }

        private string applicationVersion;
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { SetProperty(ref applicationVersion, value); }
        }

        public IDeviceService Device { get; }

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

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            await Task.Run(() => LoadData());
        }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();
                SystemInfo = GetSystemInfo();
                PeersInfo = GetSystemPeers();
                //Device.StartTimer(TimeSpan.FromSeconds(10), RefreshPeers);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool RefreshPeers()
        {
            try
            {
                PeersInfo = GetSystemPeers();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
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