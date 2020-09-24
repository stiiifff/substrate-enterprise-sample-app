using System;
using System.Numerics;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using Prism.Services;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AccountInfoViewModel : BaseViewModel
    {
        string balanceSid;

        public AccountInfoViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService device, IToastService toast,
            IAccountService accountService)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Substrate Enterprise Sample";

            Device = device;
            Toast = toast;
            AccountService = accountService;
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }
        public IAccountService AccountService { get; }
        public ICommand ReadyCommand { get; }

        private string accountName;
        public string AccountName
        {
            get { return accountName; }
            set { SetProperty(ref accountName, value); }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }

        private decimal balance;
        public decimal Balance
        {
            get { return balance; }
            set { SetProperty(ref balance, value); }
        }

        private string qrCode;
        public string QrCode
        {
            get { return qrCode; }
            set { SetProperty(ref qrCode, value); }
        }

        public override void Initialize(INavigationParameters parameters)
        {
            AccountName = Xamarin.Essentials.Preferences.Get("AccountName", null);
            Address = Xamarin.Essentials.Preferences.Get("Address", null);
            QrCode = $"substrate:{Address}:{AccountName}";

            balanceSid = PolkadotApi.SubscribeAccountInfo(Address, accountInfo =>
                Device.BeginInvokeOnMainThread(() => Balance =
                  (decimal)(accountInfo.AccountData.Free / new BigInteger(Math.Pow(10,15)))
            ));
        }

        public override void OnNavigatedFrom(INavigationParameters parameters)
        {
            if (!string.IsNullOrEmpty(balanceSid))
                PolkadotApi.UnsubscribeAccountInfo(balanceSid);
        }
    }
}
