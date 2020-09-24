using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
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
            IDeviceService device, IToastService toast, IAccountService accountService)
            : base(navigationService, lightClient, polkadotApi)
        {
            Device = device;
            Toast = toast;
            AccountService = accountService;
            TrackShipmentCommand = new Command(async (op) => await TrackShipmentAsync(Enum.Parse<ShippingOperation>((string)op)));
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }
        public IAccountService AccountService { get; }
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

        private async Task TrackShipmentAsync(ShippingOperation operation)
        {
            TransactionProgress = 0.0f;
            TransactionStatus = "";

            await Task.Run(async () =>
            {
                try
                {
                    var ser = PolkadotApi.Serializer;

                    var account = Xamarin.Essentials.Preferences.Get("AccountName", null);
                    var address = Xamarin.Essentials.Preferences.Get("Address", null);
                    var sender = new Address(address);
                    var pubkey = AddressUtils.GetPublicKeyFromAddr(sender).Bytes;

                    var secret = (await AccountService.RetrieveAccountSecretAsync(account)).ToUnsecureString();

                    //TODO: error management
                    var encodedExtrinsic = ser.Serialize(
                        new TrackShipmentCall(
                            new Identifier(ShipmentId),
                            (int)operation,
                            DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                            new ReadPoint(),
                            new ReadingList()
                        )
                    );
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
                    Toast.ShowShortToast($"Error performing {operation}");
                }
            });
        }
    }
}
