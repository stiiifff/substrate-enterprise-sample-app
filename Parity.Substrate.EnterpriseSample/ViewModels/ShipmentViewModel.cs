using System;
using System.Diagnostics;
using System.Threading.Tasks;
using OneOf;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.Utils;
using Prism.Navigation;
using Prism.Services;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class ShipmentViewModel : BaseViewModel
    {
        public ShipmentViewModel(INavigationService navigationService,
            IDeviceService device, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Device = device;
        }

        public IDeviceService Device { get; }

        private string shipmentId;
        public string ShipmentId
        {
            get { return shipmentId; }
            set { SetProperty(ref shipmentId, value, () => Title = $"Shipment {ShipmentId}"); }
        }

        private Shipment shipment;
        public Shipment Shipment
        {
            get { return shipment; }
            set { SetProperty(ref shipment, value); }
        }

        public override void Initialize(INavigationParameters parameters)
        {
            if (parameters != null && parameters.ContainsKey("shipmentId"))
                ShipmentId = parameters.GetValue<string>("shipmentId");
        }

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            await LoadDataAsync();
        }

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                await Task.Run(() =>
                {
                    try
                    {
                        var response = PolkadotApi.GetStorage(new Identifier(ShipmentId), "ProductTracking", "Shipments");
                        var maybeShipment = PolkadotApi.Serializer.Deserialize<OneOf<Shipment,Empty>>(response.HexToByteArray());
                        if (maybeShipment.IsT0)
                            Device.BeginInvokeOnMainThread(() => Shipment = maybeShipment.AsT0);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
