using Parity.Substrate.EnterpriseSample.Views;
using Polkadot.Api;
using Polkadot.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample
{
    public partial class App : Xamarin.Forms.Application
    {
        const string NodeUrl = "ws://127.0.0.1:9944";

        public App()
        {
            InitializeComponent();
            MainPage = new MainPage();
        }

        public IJsonRpc PolkadotApi => DependencyService.Get<IJsonRpc>();

        public bool IsPolkadotApiConnected { get; private set; }

        public void ConnectToNode()
        {
            if (IsPolkadotApiConnected)
                return;

            try
            {
                var _res = PolkadotApi.Connect(NodeUrl);
                IsPolkadotApiConnected = true;
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        protected override void OnSleep()
        {
            base.OnSleep();
            IsPolkadotApiConnected = false;
        }
    }
}
