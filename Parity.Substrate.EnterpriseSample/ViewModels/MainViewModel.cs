using System;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using Prism.Services;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        public MainViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService deviceService, INodeService nodeService)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Substrate Enterprise Sample";

            nodeService.BestBlock.Subscribe(block =>
                deviceService.BeginInvokeOnMainThread(() =>
                    BestBlock = $"best #{block}"));
        }

        private string bestBlock;
        public string BestBlock
        {
            get { return bestBlock; }
            set { SetProperty(ref bestBlock, value); }
        }
    }
}
