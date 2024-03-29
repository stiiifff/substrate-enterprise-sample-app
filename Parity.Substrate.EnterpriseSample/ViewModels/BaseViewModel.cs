﻿using System.Linq;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Mvvm;
using Prism.Navigation;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public abstract class BaseViewModel : BindableBase, IInitialize, INavigationAware, IDestructible
    {
        bool isBusy = false;
        bool isReady = true;

        public BaseViewModel(INavigationService navigationService, ILightClient lightClient, IApplication polkadotApi)
        {
            NavigationService = navigationService;
            LightClient = lightClient;
            PolkadotApi = polkadotApi;
        }

        protected App App => ((App)Xamarin.Forms.Application.Current);
        protected INavigationService NavigationService { get; private set; }
        public ILightClient LightClient { get; }
        public IApplication PolkadotApi { get; }

        public bool IsBusy
        {
            get { return isBusy; }
            set { 
                SetProperty(ref isBusy, value);
                IsReady = !value;
            }
        }

        public bool IsReady
        {
            get { return isReady; }
            private set { SetProperty(ref isReady, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        public virtual void Initialize(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedFrom(INavigationParameters parameters)
        {

        }

        public virtual void OnNavigatedTo(INavigationParameters parameters)
        {

        }

        public virtual void Destroy()
        {

        }

        protected T GetValueFromStorageMap<T>(string module, string storageMap, object key)
        {
            var param = PolkadotApi.Serializer.Serialize(key);
            var paramKey = Hash.GetStorageKey(Hasher.BLAKE2, param, param.Length, PolkadotApi.Serializer);

            var response = PolkadotApi.GetStorage(paramKey.Concat(param).ToArray(), module, storageMap);
            return PolkadotApi.Serializer.Deserialize<T>(response.HexToByteArray());
        }
    }
}
