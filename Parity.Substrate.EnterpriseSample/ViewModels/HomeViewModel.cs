using Newtonsoft.Json.Linq;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using System;
using System.Linq;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;
using System.Globalization;
using Polkadot.Source.Utils;
using Polkadot.Utils;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(INavigationService navigationService, ILightClient lightClient, IJsonRpc polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Home";
            QueryChainStateCommand = new Command(QueryChainState);
        }

        public ICommand QueryChainStateCommand { get; }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void QueryChainState()
        {
            try
            {
                var client = Polkadot.Api.PolkaApi.GetAppication();
                client.Connect("ws://127.0.0.1:9944");
                //var now = client.GetStorage("Timestamp", "Now");
                var response = client.GetStorage<Address>(new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY"), "ProductRegistry", "ProductsOfOrganization");
                //var result = Converters.FromHex(JObject.Parse(response)["result"].ToString());
                Trace.WriteLine(response);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
