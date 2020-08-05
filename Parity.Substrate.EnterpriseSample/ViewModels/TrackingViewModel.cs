using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Events;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class TrackingViewModel : TabViewModel
    {
        readonly SubscriptionToken eventSubs;

        public TrackingViewModel(INavigationService navigationService,
            IDeviceService device, ILightClient lightClient, IApplication polkadotApi,
            IEventAggregator eventAggregator)
            : base(navigationService, lightClient, polkadotApi)
        {
            Device = device;
            EventAggregator = eventAggregator;
            Title = "Track";            
            RefreshCommand = new Command(async () => await RefreshAsync());

            IsActiveChanged += OnIsActiveChanged;

            eventSubs = EventAggregator.GetEvent<ApiStatusEvent>().Subscribe(async status =>
            {
                if (status == ApiStatus.ApiReady)
                    await LoadDataAsync();
            });
        }

        public IDeviceService Device { get; }
        public IEventAggregator EventAggregator { get; }
        public ICommand RefreshCommand { get; }

        bool isRefreshing;
        public bool IsRefreshing
        {
            get { return isRefreshing; }
            set { SetProperty(ref isRefreshing, value); }
        }

        private ObservableCollection<ShipmentInfoViewModel> shipments = new ObservableCollection<ShipmentInfoViewModel>();
        public ObservableCollection<ShipmentInfoViewModel> Shipments
        {
            get { return shipments; }
            set { SetProperty(ref shipments, value); }
        }

        ShipmentInfoViewModel selectedShipment;
        public ShipmentInfoViewModel SelectedShipment
        {
            get { return selectedShipment; }
            set
            {
                SetProperty(ref selectedShipment, value, onChanged: () => {
                    if (SelectedShipment == null)
                        return;
                    Task.Delay(100).ContinueWith(_ => Device.BeginInvokeOnMainThread(
                      async () => await NavigationService.NavigateAsync(
                          "ShipmentPage?shipmentId=" + SelectedShipment.ShipmentId))
                    );
                });
            }
        }

        async void OnIsActiveChanged(object sender, EventArgs e)
        {
            if (IsActive && App.IsPolkadotApiConnected)
                await LoadDataAsync();
        }

        async Task RefreshAsync()
        {

            try
            {
                await LoadDataAsync();
            }
            finally
            {
                IsRefreshing = false;
            }
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
                        var response = PolkadotApi.GetStorage(new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY"), "ProductTracking", "ShipmentsOfOrganization");
                        var shipments = PolkadotApi.Serializer.Deserialize<ShipmentIdList>(response.HexToByteArray());
                        var shipmentsObs = new ObservableCollection<ShipmentInfoViewModel>(shipments.ShipmentIds.Select(s => new ShipmentInfoViewModel(s.ToString())));
                        Device.BeginInvokeOnMainThread(() => Shipments = shipmentsObs);
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

        public override void Destroy()
        {
            if (eventSubs != null)
                EventAggregator.GetEvent<ApiStatusEvent>().Unsubscribe(eventSubs);
        }
    }
}
