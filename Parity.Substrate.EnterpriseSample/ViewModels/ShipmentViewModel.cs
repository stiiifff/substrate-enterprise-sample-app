using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

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
            PickupShipmentCommand = new Command(PickupShipmentAsync);
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }

        public ICommand PickupShipmentCommand { get; }

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

        private async void PickupShipmentAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var ser = PolkadotApi.Serializer;

                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        //var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                        var shipmentId = ser.Serialize(new Identifier(ShipmentId));
                        var operation = Scale.EncodeCompactInteger(new BigInteger((int)ShippingOperation.Pickup));
                        var timestamp = Scale.EncodeCompactInteger(new BigInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                        var emptyArgs = new byte[2];

                        var encodedExtrinsic = new byte[shipmentId.Length + operation.Length + timestamp.Length + emptyArgs.Length];
                        shipmentId.CopyTo(encodedExtrinsic.AsMemory());
                        operation.Bytes.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length));
                        timestamp.Bytes.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length + (int)operation.Length));
                        emptyArgs.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length + (int)operation.Length + (int)timestamp.Length));

                        //var encodedExtrinsic = ser.Serialize(new TrackShipmentCall(
                        //    new Identifier(ShipmentId),
                        //    new byte(), // One zero byte == ShippingOperation.Pickup,
                        //    DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                        //    new ReadPoint { Latitude = 0, Longitude = 0 },
                        //    new ReadingList()));

                        Trace.WriteLine(encodedExtrinsic.ToPrefixedHexString());

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SubmitAndSubcribeExtrinsic(encodedExtrinsic,
                            "ProductTracking", "track_shipment", sender, secret, str =>
                            {
                                Trace.WriteLine(str);
                                tcs.SetResult(str);
                            });

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                        PolkadotApi.UnsubscribeStorage(sid);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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
    }
}
