using System;
using System.Collections.Generic;
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
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService device, IToastService toast)
            : base(navigationService, lightClient, polkadotApi)
        {
            Device = device;
            Toast = toast;
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }

        private string shipmentId;
        public string ShipmentId
        {
            get { return shipmentId; }
            set { SetProperty(ref shipmentId, value, () => Title = $"Shipment {ShipmentId}"); }
        }

        private ShipmentInfo shipment;
        public ShipmentInfo Shipment
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
            if (Shipment == null)
                await LoadDataAsync();
        }

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var param = PolkadotApi.Serializer.Serialize(new Identifier(ShipmentId));
                        var paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                        var response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductTracking", "Shipments");
                        var storedShipment = PolkadotApi.Serializer.Deserialize<Shipment>(response.HexToByteArray());

                        var products = new List<ProductInfo>();
                        foreach (var productId in storedShipment.Products.ProductIds)
                        {
                            param = PolkadotApi.Serializer.Serialize(productId);
                            paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                            response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductRegistry", "Products");
                            var storedProduct = PolkadotApi.Serializer.Deserialize<Product>(response.HexToByteArray());
                            products.Add(new ProductInfo
                            {
                                ProductId = productId.ToString(),
                                Owner = AddressUtils.GetAddrFromPublicKey(storedProduct.Owner),
                                Props = storedProduct.PropList.IsT1
                                    ? storedProduct.PropList.AsT1.Props.Select(p =>
                                        new ProductPropertyInfo { Name = p.Name.ToString(), Value = p.Value.ToString() }).ToArray()
                                    : new ProductPropertyInfo[0],
                                Registered = DateTimeOffset.FromUnixTimeMilliseconds(storedProduct.Registered).LocalDateTime,
                            });
                        }

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            Shipment =
                                new ShipmentInfo
                                {
                                    ShipmentId = ShipmentId,
                                    Owner = AddressUtils.GetAddrFromPublicKey(storedShipment.Owner),
                                    Status = storedShipment.Status.Value.ToString(),
                                    Products = products.ToArray(),
                                    Registered = DateTimeOffset.FromUnixTimeMilliseconds(storedShipment.Registered).LocalDateTime,
                                    Delivered = storedShipment.Delivered.IsT1
                                        ? DateTimeOffset.FromUnixTimeMilliseconds((long)storedShipment.Delivered.Value).LocalDateTime
                                        : (DateTime?)null
                                };
                            ShipmentOperationsVisible = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        Toast.ShowShortToast("Error loading shipment.");
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
