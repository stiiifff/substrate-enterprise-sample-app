using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Navigation;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        public HomeViewModel(INavigationService navigationService, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Home";
            QueryChainStateCommand = new Command(QueryChainState);
            SubmitExtrinsicCommand = new Command(async () => await SubmitExtrinsicAsync());
        }

        public ICommand QueryChainStateCommand { get; }
        public ICommand SubmitExtrinsicCommand { get; }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        private void QueryChainState()
        {
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                var response = PolkadotApi.GetStorage(new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY"), "ProductRegistry", "ProductsOfOrganization");
                var products = PolkadotApi.Serializer.Deserialize<ProductIdList>(response.HexToByteArray());
                foreach (var product in products.ProductIds)
                    Trace.WriteLine(product);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private async Task SubmitExtrinsicAsync()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                var ser = PolkadotApi.Serializer;

                var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                var pvk = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";
                var shipmentId = ser.Serialize(new Identifier("S0002"));
                var operation = Scale.EncodeCompactInteger(new BigInteger((int)ShippingOperation.Scan));
                var timestamp = Scale.EncodeCompactInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                var emptyArgs = new byte[2];

                var buf = new byte[pub.Bytes.Length + shipmentId.Length + operation.Length + timestamp.Length + emptyArgs.Length];
                pub.Bytes.CopyTo(buf.AsMemory());
                shipmentId.CopyTo(buf.AsMemory(pub.Bytes.Length));
                operation.Bytes.CopyTo(buf.AsMemory(pub.Bytes.Length + shipmentId.Length));
                timestamp.Bytes.CopyTo(buf.AsMemory(pub.Bytes.Length + shipmentId.Length + (int)operation.Length));
                emptyArgs.CopyTo(buf.AsMemory(pub.Bytes.Length + shipmentId.Length + (int)operation.Length + (int)timestamp.Length));

                var tcs = new TaskCompletionSource<string>();
                PolkadotApi.SubmitAndSubcribeExtrinsic(buf, "ProductTracking", "TrackShipment", sender, pvk, str => tcs.SetResult(str));

                var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30));
                Trace.WriteLine(result);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
