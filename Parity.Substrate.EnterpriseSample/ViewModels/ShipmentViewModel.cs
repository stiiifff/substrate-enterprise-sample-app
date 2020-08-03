
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class ShipmentViewModel : BaseViewModel
    {
        public ShipmentViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi, string shipmentId)
            : base(navigationService, lightClient, polkadotApi)
        {
            ShipmentId = shipmentId;
        }

        private string shipmentId;
        public string ShipmentId
        {
            get { return shipmentId; }
            set { SetProperty(ref shipmentId, value); }
        }
    }
}
