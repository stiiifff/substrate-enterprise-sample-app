using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
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

        private ShipmentInfoViewModel shipment;
        public ShipmentInfoViewModel Shipment
        {
            get { return shipment; }
            set { SetProperty(ref shipment, value); }
        }

        private bool shipmentOperationsVisible;
        public bool ShipmentOperationsVisible
        {
            get { return shipmentOperationsVisible; }
            set { SetProperty(ref shipmentOperationsVisible, value); }
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
                        var param = PolkadotApi.Serializer.Serialize(new Identifier(ShipmentId));
                        var paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                        var response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductTracking", "Shipments");
                        var storedShipment = PolkadotApi.Serializer.Deserialize<Shipment>(response.HexToByteArray());
                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Shipment =
                                new ShipmentInfoViewModel(
                                    shipmentId: ShipmentId,
                                    owner: AddressUtils.GetAddrFromPublicKey(storedShipment.Owner),
                                    status: storedShipment.Status.Value.ToString(),
                                    products: storedShipment.Products.ProductIds.Select(id => id.ToString()).ToArray(),
                                    registered: new DateTime(long.Parse(storedShipment.Registered.ToString()))
                                );
                            ShipmentOperationsVisible = true;
                        });
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
