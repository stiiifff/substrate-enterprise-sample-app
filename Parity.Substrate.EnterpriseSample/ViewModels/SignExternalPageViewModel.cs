using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.DataStructs;
using Polkadot.DataStructs.Metadata;
using Polkadot.Utils;
using Prism.Navigation;
using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class SignExternalPageViewModel : BaseViewModel
    {
        public SignExternalPageViewModel(INavigationService navigationService,
            ILightClient lightClient, IApplication polkadotApi, IAccountService accountService)
            : base(navigationService, lightClient, polkadotApi)
        {
            AccountService = accountService;
        }

        private string unsignedTx;
        public string UnsignedTx
        {
            get { return unsignedTx; }
            set { SetProperty(ref unsignedTx, value); }
        }

        public IAccountService AccountService { get; }

        //public override void Initialize(INavigationParameters parameters)
        //{
        //    if (parameters != null && parameters.ContainsKey("unsignedTx"))
        //        UnsignedTx = parameters.GetValue<string>("unsignedTx");
        //}

        public override async void OnNavigatedTo(INavigationParameters parameters)
        {
            if (string.IsNullOrEmpty(UnsignedTx))
                await LoadDataAsync();
        }

        internal async Task LoadDataAsync()
        {
            IsBusy = true;
            try
            {
                await Task.Run(async () =>
                {
                    try
                    {
                        var (addr, mnemonic, secret) = await AccountService.GenerateSr25519KeyPairAsync("ANDROID", "Substrate");

                        //Device.BeginInvokeOnMainThread(() => UnsignedTx = $"substrate:{addr.Symbols}");

                        var ser = PolkadotApi.Serializer;
                        var signer = PolkadotApi.Signer;

                        var module = "ProductTracking";
                        var method = "track_shipment";
                        var sender = addr;
                        //var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        //var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                        var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var address = new ExtrinsicAddress(pub);

                        var encodedExtrinsic = ser.Serialize(
                            new TrackShipmentCall(
                                new Identifier("S0002"),
                                (int)ShippingOperation.Scan,
                                DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                                new ReadPoint(),
                                new ReadingList()
                            )
                        );

                        // Get account Nonce
                        var nonce = PolkadotApi.GetAccountNonce(sender);
                        Trace.WriteLine($"sender nonce: {nonce}");
                        var extra = new SignedExtra(new EraDto(new ImmortalEra()), nonce, BigInteger.Zero);

                        var metadata = new Metadata(PolkadotApi.GetMetadata(null));
                        var absoluteIndex = metadata.GetModuleIndex(module, false);
                        var moduleIndex = (byte)metadata.GetModuleIndex(module, true);
                        var methodIndex = (byte)metadata.GetCallMethodIndex(absoluteIndex, method);

                        var call = new ExtrinsicCallRaw<byte[]>(moduleIndex, methodIndex, encodedExtrinsic);
                        var extrinsic = new UncheckedExtrinsic<ExtrinsicAddress, ExtrinsicMultiSignature, SignedExtra, ExtrinsicCallRaw<byte[]>>(true, address, null, extra, call);
                        var payload = signer.GetSignaturePayload(extrinsic);

                        Device.BeginInvokeOnMainThread(() => UnsignedTx = payload.ToHexString());

                        signer.SignUncheckedExtrinsic(extrinsic, AddressUtils.GetPublicKeyFromAddr(sender).Bytes, secret.HexToByteArray());
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                        //Toast.ShowShortToast("Error loading shipment.");
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
