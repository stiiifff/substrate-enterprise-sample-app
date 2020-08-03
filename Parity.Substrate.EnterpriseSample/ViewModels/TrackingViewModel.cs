using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Navigation;
using Prism.Services;
class MyClass
{

}
namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class TrackingViewModel : BaseViewModel
    {
        public TrackingViewModel(INavigationService navigationService, IDeviceService device, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Tracking";
        }

        private ObservableCollection<ShipmentViewModel> shipments = new ObservableCollection<ShipmentViewModel>();
        public ObservableCollection<ShipmentViewModel> Shipments
        {
            get { return shipments; }
            set { SetProperty(ref shipments, value); }
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            base.OnNavigatedTo(parameters);
            await Task.Run(() => LoadData());
        }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                var response = PolkadotApi.GetStorage(new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY"), "ProductRegistry", "ProductsOfOrganization");
                var shipments = PolkadotApi.Serializer.Deserialize<ShipmentIdList>(response.HexToByteArray());
                Shipments = new ObservableCollection<ShipmentViewModel>(shipments.ShipmentIds.Select(s =>
                    new ShipmentViewModel(NavigationService, LightClient, PolkadotApi, s.ToString())));
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

