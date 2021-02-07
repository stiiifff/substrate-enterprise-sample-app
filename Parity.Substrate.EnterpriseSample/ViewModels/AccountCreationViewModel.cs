using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism.Navigation;
using Prism.Services;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AccountCreationViewModel : BaseViewModel
    {
        public AccountCreationViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi,
            IDeviceService device, IToastService toast,
            IAccountService accountService)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Substrate Enterprise Sample";

            Device = device;
            Toast = toast;
            AccountService = accountService;
            CreateAccountCommand = new Command(
                async () => await CreateAccountAsync(),
                () => !string.IsNullOrWhiteSpace(AccountName) && !string.IsNullOrWhiteSpace(AccountPassword)
            );
        }

        public IDeviceService Device { get; }
        public IToastService Toast { get; }
        public IAccountService AccountService { get; }
        public Command CreateAccountCommand { get; }

        private string accountName;
        public string AccountName
        {
            get { return accountName; }
            set { SetProperty(ref accountName, value, () => CreateAccountCommand.ChangeCanExecute()); }
        }

        private string accountPassword;
        public string AccountPassword
        {
            get { return accountPassword; }
            set { SetProperty(ref accountPassword, value, () => CreateAccountCommand.ChangeCanExecute()); }
        }

        private async Task CreateAccountAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var (address, mnemonic) = await AccountService.GenerateSr25519KeyPairAsync(AccountName, AccountPassword);
                        await Task.Delay(1000);

                        Device.BeginInvokeOnMainThread(async () =>
                            await NavigationService.NavigateAsync("AccountMnemonicPage", new NavigationParameters
                            {
                                {"accountName", AccountName},
                                {"address", address.Symbols },
                                {"mnemonic", mnemonic }
                            })
                        );
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        Toast.ShowShortToast($"Error creating account.");
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
