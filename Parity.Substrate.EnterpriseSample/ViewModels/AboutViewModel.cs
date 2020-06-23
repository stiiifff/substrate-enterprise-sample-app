using Polkadot.Data;
using System;
using System.Diagnostics;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
            Device.StartTimer(TimeSpan.FromSeconds(10), () => { RefreshPeers(); return true; });
        }

        private string applicationVersion;
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { SetProperty(ref applicationVersion, value); }
        }

        private PeersInfo peersInfo;
        public PeersInfo PeersInfo
        {
            get { return peersInfo; }
            set { SetProperty(ref peersInfo, value); }
        }

        private RuntimeVersion runtimeVersion;
        public RuntimeVersion RuntimeVersion
        {
            get { return runtimeVersion; }
            set { SetProperty(ref runtimeVersion, value); }
        }

        private SystemInfo systemInfo;
        public SystemInfo SystemInfo
        {
            get { return systemInfo; }
            set { SetProperty(ref systemInfo, value); }
        }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
                
                SystemInfo = PolkadotApi.GetSystemInfo();
                //RuntimeVersion = PolkadotApi.GetRuntimeVersion(new GetRuntimeVersionParams { });
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

        private void RefreshPeers()
        {
            try
            {
                PeersInfo = PolkadotApi.GetSystemPeers();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
        }
    }
}