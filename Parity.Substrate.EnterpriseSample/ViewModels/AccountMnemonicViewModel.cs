using System.Threading.Tasks;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AccountMnemonicViewModel : BaseViewModel
    {
        public AccountMnemonicViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService device, IToastService toast,
            IAccountService accountService)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Substrate Enterprise Sample";

            Device = device;
            Toast = toast;
            AccountService = accountService;
            ReadyCommand = new Command(async () => await ReadyAsync());
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

        private string mnemonic;
        public string Mnemonic
        {
            get { return mnemonic; }
            set { SetProperty(ref mnemonic, value); }
        }

        private string address;
        public string Address
        {
            get { return address; }
            set { SetProperty(ref address, value); }
        }

        public override void Initialize(INavigationParameters parameters)
        {
            if (parameters != null)
            {
                if (parameters.ContainsKey("accountName"))
                    AccountName = parameters.GetValue<string>("accountName");
                if (parameters.ContainsKey("address"))
                    Address = parameters.GetValue<string>("address");
                if (parameters.ContainsKey("mnemonic"))
                    Mnemonic = parameters.GetValue<string>("mnemonic");
            }
        }

        private async Task ReadyAsync()
        {
            Xamarin.Essentials.Preferences.Set("IsFirstRun", false);
            Xamarin.Essentials.Preferences.Set("AccountName", AccountName);
            Xamarin.Essentials.Preferences.Set("Address", Address);
            await NavigationService.NavigateAsync("/NavigationPage/MainPage");
        }
    }
}
