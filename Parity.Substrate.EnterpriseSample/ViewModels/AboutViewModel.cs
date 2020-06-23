using Polkadot.Data;
using System;
using System.Diagnostics;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
        }

        private string applicationVersion;
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { SetProperty(ref applicationVersion, value); }
        }

        private SystemInfo systemInfo;
        public SystemInfo SystemInfo
        {
            get { return systemInfo; }
            set { SetProperty(ref systemInfo, value); }
        }

        private RuntimeVersion runtimeVersion;
        public RuntimeVersion RuntimeVersion
        {
            get { return runtimeVersion; }
            set { SetProperty(ref runtimeVersion, value); }
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
    }
}