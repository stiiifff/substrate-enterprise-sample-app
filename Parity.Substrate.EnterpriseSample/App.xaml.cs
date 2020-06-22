using Parity.Substrate.EnterpriseSample.Views;
using Polkadot.Api;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample
{
    public partial class App : Xamarin.Forms.Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        public IApplication PolkadotApi => DependencyService.Get<IApplication>();

        protected override void OnStart()
        {
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
