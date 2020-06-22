using Polkadot.Api;
using System;
using System.ComponentModel;
using System.Diagnostics;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample
{
    // Learn more about making custom code visible in the Xamarin.Forms previewer
    // by visiting https://aka.ms/xamarinforms-previewer
    [DesignTimeVisible(false)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public IApplication PolkadotApi => DependencyService.Get<IApplication>();

        private void Button_Clicked(object sender, EventArgs e)
        {
            try
            {
                var systemInfo = PolkadotApi.GetSystemInfo();
                label.Text = systemInfo.Version;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
