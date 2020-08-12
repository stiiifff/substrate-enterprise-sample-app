using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
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

        EventIndexList GetShippingEvents(string shipmentId) => GetValueFromStorageMap<EventIndexList>("ProductTracking", "EventsOfShipment", new Identifier(shipmentId));

        ShippingEvent GetShippingEvent(EventIndex eventIdx) => GetValueFromStorageMap<ShippingEvent>("ProductTracking", "AllEvents", eventIdx);

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(() =>
                {
                    try
                    {
                        var eventIndexList = GetShippingEvents(ShipmentId);

                        var events = new List<ShippingEvent>();
                        foreach (var eventIdx in eventIndexList.EventIndices)
                        {
                            var @event = GetShippingEvent(eventIdx);
                            events.Add(@event);
                        }

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
