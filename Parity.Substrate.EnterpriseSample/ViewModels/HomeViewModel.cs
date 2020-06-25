using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using System.Diagnostics;
using System.Windows.Input;
using Xamarin.Forms;

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
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }
    }
}
