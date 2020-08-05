using System;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Prism;
using Prism.Navigation;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public abstract class TabViewModel : BaseViewModel, IActiveAware
    {
        public TabViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
        }

        public event EventHandler IsActiveChanged;

        bool isActive;
        public bool IsActive
        {
            get { return isActive; }
            set { SetProperty(ref isActive, value, RaiseIsActiveChanged); }
        }

        protected virtual void RaiseIsActiveChanged()
        {
            IsActiveChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
