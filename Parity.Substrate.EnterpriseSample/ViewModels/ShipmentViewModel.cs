using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
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
        private string transactionSid;

        public ShipmentViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService device, IToastService toast)
            : base(navigationService, lightClient, polkadotApi)
        {
            Device = device;
            Toast = toast;
            TrackShipmentCommand = new Command(async (op) => await TrackShipmentAsync(Enum.Parse<ShippingOperation>((string)op)));
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }

        public ICommand TrackShipmentCommand { get; }

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

        private bool transactionInProgress;
        public bool TransactionInProgress
        {
            get { return transactionInProgress; }
            set { SetProperty(ref transactionInProgress, value); }
        }

        private string transactionStatus;
        public string TransactionStatus
        {
            get { return transactionStatus; }
            set { SetProperty(ref transactionStatus, value); }
        }

        public float transactionProgress;
        public float TransactionProgress
        {
            get { return transactionProgress; }
            set { SetProperty(ref transactionProgress, value); }
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

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (!string.IsNullOrEmpty(transactionSid))
                PolkadotApi.UnsubscribeStorage(transactionSid);
        }

        T GetValueFromStorageMap<T>(string module, string storageMap, object key)
        {
            var param = PolkadotApi.Serializer.Serialize(key);
            var paramKey = Hash.GetStorageKey(Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

            var response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), module, storageMap);
            return PolkadotApi.Serializer.Deserialize<T>(response.HexToByteArray());
        }

        Shipment GetShipment(string shipmentId) => GetValueFromStorageMap<Shipment>("ProductTracking", "Shipments", new Identifier(shipmentId));

        Product GetProduct(string productId) => GetValueFromStorageMap<Product>("ProductRegistry", "Products", new Identifier(productId));

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var storedShipment = GetShipment(ShipmentId);
                        var products = new List<ProductInfo>();
                        foreach (var productId in storedShipment.Products.ProductIds)
                        {
                            var storedProduct = GetProduct(productId.ToString());
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

        private async Task TrackShipmentAsync(ShippingOperation op)
        {
            TransactionProgress = 0.0f;
            TransactionStatus = "";

            await Task.Run(() =>
            {
                try
                {
                    var ser = PolkadotApi.Serializer;

                    //TODO: Implement account management
                    var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                    var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                    var shipmentId = ser.Serialize(new Identifier(ShipmentId));
                    //TODO: ProductTracking pallet should accept compact scale-encoded operation
                    //var operation = Scale.EncodeCompactInteger(new BigInteger((int)op));
                    var operation = new[] { Convert.ToByte(op) };

                    //TODO: ProductTracking pallet should accept compact scale-encoded timestamp
                    //var timestamp = Scale.EncodeCompactInteger(new BigInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                    var timestamp = BitConverter.GetBytes(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()).ToArray();

                    //TODO: Allow to capture location, and input readings. Empty for now.
                    var emptyArgs = new byte[2];

                    var encodedExtrinsic = new byte[shipmentId.Length + operation.Length + timestamp.Length + emptyArgs.Length];
                    shipmentId.CopyTo(encodedExtrinsic.AsMemory());
                    operation.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length));
                    timestamp.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length + (int)operation.Length));
                    emptyArgs.CopyTo(encodedExtrinsic.AsMemory(shipmentId.Length + (int)operation.Length + (int)timestamp.Length));

                    Trace.WriteLine(encodedExtrinsic.ToPrefixedHexString());

                    transactionSid = PolkadotApi.SubmitAndSubcribeExtrinsic(encodedExtrinsic,
                        "ProductTracking", "track_shipment", sender, secret, response =>
                        {
                            Trace.WriteLine(response);

                            var res = JObject.Parse(response);
                            if (res.Value<string>("subscription") != transactionSid ||
                                !res.ContainsKey("result"))
                                return;

                            Device.BeginInvokeOnMainThread(() =>
                            {
                                TransactionInProgress = true;

                                var result = res["result"];
                                if (result is JValue value && (string)value.Value == "ready")
                                {
                                    TransactionStatus = "valid";
                                    TransactionProgress = 0.25f;
                                }
                                else if (result is JObject resObj)
                                {
                                    if (resObj.ContainsKey("broadcast"))
                                    {
                                        TransactionStatus = "broadcasted to peers";
                                        TransactionProgress = 0.5f;
                                    }
                                    else if (resObj.ContainsKey("inBlock"))
                                    {
                                        TransactionStatus = "in block " + resObj.Value<string>("inBlock");
                                        TransactionProgress = 0.75f;
                                    }
                                    else if (resObj.ContainsKey("finalized"))
                                    {
                                        PolkadotApi.UnsubscribeStorage(transactionSid);
                                        transactionSid = null;
                                        TransactionStatus = "finalized";
                                        TransactionProgress = 1.0f;

                                        _ = Task.Delay(2000).ContinueWith(_ =>
                                            Device.BeginInvokeOnMainThread(() =>
                                                TransactionInProgress = false));
                                    }
                                }
                            });
                        });
                }
                catch (Exception ex)
                {
                    Trace.WriteLine(ex);
                    Toast.ShowShortToast($"Error performing {op}");
                }
            });
        }
    }
}
