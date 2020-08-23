using System;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

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

            BestBlockCommand = new Command(OpenSignExternalPage);
        }

        public ICommand BestBlockCommand { get; }

        private string bestBlock;
        public string BestBlock
        {
            get { return bestBlock; }
            set { SetProperty(ref bestBlock, value); }
        }

        private async void OpenSignExternalPage()
        {
            await NavigationService.NavigateAsync("SignExternalPage");
        }
    }
}
