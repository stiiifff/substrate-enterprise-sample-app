using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class ShipmentJourneyViewModel : BaseViewModel
    {
        public ShipmentJourneyViewModel(INavigationService navigationService,
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
            set { SetProperty(ref shipmentId, value, () => Title = $"Shipment {ShipmentId} - Journey"); }
        }

        private ObservableCollection<ShippingEventInfo> shippingEvents = new ObservableCollection<ShippingEventInfo>();
        public ObservableCollection<ShippingEventInfo> ShippingEvents
        {
            get { return shippingEvents; }
            set { SetProperty(ref shippingEvents, value); }
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
                await Task.Run(() =>
                {
                    try
                    {
                        var param = PolkadotApi.Serializer.Serialize(new Identifier(ShipmentId));
                        var paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                        var response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductTracking", "EventsOfShipment");
                        var eventIndexList = PolkadotApi.Serializer.Deserialize<EventIndexList>(response.HexToByteArray());

                        var events = new List<ShippingEvent>();
                        foreach (var eventIdx in eventIndexList.EventIndices)
                        {
                            param = eventIdx.Value;
                            paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                            response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductTracking", "AllEvents");
                            var @event = PolkadotApi.Serializer.Deserialize<ShippingEvent>(response.HexToByteArray());

                            events.Add(@event);
                        }

                        //var products = new List<ProductInfo>();
                        //foreach (var productId in storedShipment.Products.ProductIds)
                        //{
                        //    param = PolkadotApi.Serializer.Serialize(productId);
                        //    paramKey = Hash.GetStorageKey(Polkadot.DataStructs.Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

                        //    response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), "ProductRegistry", "Products");
                        //    var storedProduct = PolkadotApi.Serializer.Deserialize<Product>(response.HexToByteArray());
                        //    products.Add(new ProductInfo
                        //    {
                        //        ProductId = productId.ToString(),
                        //        Owner = AddressUtils.GetAddrFromPublicKey(storedProduct.Owner),
                        //        Props = storedProduct.PropList.IsT1
                        //            ? storedProduct.PropList.AsT1.Props.Select(p =>
                        //                new ProductPropertyInfo { Name = p.Name.ToString(), Value = p.Value.ToString() }).ToArray()
                        //            : new ProductPropertyInfo[0],
                        //        Registered = DateTimeOffset.FromUnixTimeMilliseconds(storedProduct.Registered).LocalDateTime,
                        //    });
                        //}

                        Device.BeginInvokeOnMainThread(() =>
                        {
                            ShippingEvents = new ObservableCollection<ShippingEventInfo>(events.Select(e =>
                                new ShippingEventInfo
                                {
                                    Type = e.Type.Value.ToString(),
                                    ShipmentId = e.ShipmentId.ToString(),
                                    Location = e.Location.IsT1
                                        ? new ReadPointInfo
                                        {
                                            Latitude = e.Location.AsT1.Latitude,
                                            Longitude = e.Location.AsT1.Longitude,
                                        } : null,
                                    Readings = e.Readings.IsT1
                                        ? e.Readings.AsT1.Readings.Select(r => new ReadingInfo
                                        {
                                            DeviceId = r.DeviceId.ToString(),
                                            ReadingType = r.Value.ToString(),
                                            Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(r.Timestamp).LocalDateTime,
                                            Value = r.Value
                                        }).ToArray() : null,
                                    Timestamp = DateTimeOffset.FromUnixTimeMilliseconds(e.Timestamp).LocalDateTime,
                                }).OrderBy(e => e.Timestamp));
                            //ShipmentEventsVisible = true;
                        });
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        Toast.ShowShortToast("Error loading shipping events.");
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
