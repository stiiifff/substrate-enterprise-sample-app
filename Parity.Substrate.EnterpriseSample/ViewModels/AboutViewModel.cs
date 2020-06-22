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