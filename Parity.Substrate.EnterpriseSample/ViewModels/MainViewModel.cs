using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Substrate Enterprise Sample";
        }
    }
}
