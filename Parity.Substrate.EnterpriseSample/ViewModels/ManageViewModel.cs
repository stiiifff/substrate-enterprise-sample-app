using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading.Tasks;
using System.Windows.Input;
using Blake2Core;
using Parity.Substrate.EnterpriseSample.Models;
using Parity.Substrate.EnterpriseSample.Services;
using Polkadot.Api;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.DataStructs;
using Polkadot.Utils;
using Prism.Navigation;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class ManageViewModel : TabViewModel
    {
        public ManageViewModel(INavigationService navigationService, ILightClient lightClient, IApplication polkadotApi)
            : base(navigationService, lightClient, polkadotApi)
        {
            Title = "Manage";
            QueryChainStateCommand = new Command(QueryChainState);
            SignTransferCommand = new Command(SubmitTransferExtrinsic);
            SubmitExtrinsicCommand = new Command(SubmitRegisterShipmentExtrinsicAsync);
        }

        public ICommand QueryChainStateCommand { get; }
        public ICommand SignTransferCommand { get; }
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

                var extrinsicExtensions = PolkadotApi.GetMetadata(new Polkadot.Data.GetMetadataParams()).GetExtrinsicExtension();
                foreach (var ext in extrinsicExtensions)
                    Trace.WriteLine(ext);
            }
            catch (System.Exception ex)
            {
                Trace.WriteLine(ex);
            }
        }

        private void SignTransfer()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        //var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";
                        var recipient = "5Ef1wcrhb5CVyZjpdYh9Keg81tgUPcsYi9uhNHC9uqjo7956";
                        var amount = BigInteger.Parse("10");

                        var tmp = Blake2B.ComputeHash(secret.HexToByteArray());

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SignAndSendTransfer(sender.Symbols, secret, recipient, amount, res => tcs.SetResult(res));
                        Trace.WriteLine(sid);

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30));
                        PolkadotApi.UnsubscribeStorage(sid);
                        Trace.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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

        private void SubmitTransferExtrinsic()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        //var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";
                        var recipient = "5Ef1wcrhb5CVyZjpdYh9Keg81tgUPcsYi9uhNHC9uqjo7956";

                        var pub = AddressUtils.GetPublicKeyFromAddr(recipient);
                        var compactAmount = Scale.EncodeCompactInteger(BigInteger.Parse("10"));
                        
                        var buf = new byte[pub.Bytes.Length + compactAmount.Bytes.Length];
                        pub.Bytes.CopyTo(buf.AsMemory());
                        compactAmount.Bytes.CopyTo(buf.AsMemory(pub.Bytes.Length));
                        Trace.WriteLine("Transfer Buf: " + buf.ToPrefixedHexString());

                        var call = new TransferCall(pub, BigInteger.Parse("10"));
                        var encodedCall = PolkadotApi.Serializer.Serialize(call);
                        Trace.WriteLine("Transfer Call: " + encodedCall.ToPrefixedHexString());

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SubmitAndSubcribeExtrinsic(encodedCall, "balances", "transfer", sender, secret, str => tcs.SetResult(str));
                        Trace.WriteLine(sid);

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30));
                        PolkadotApi.UnsubscribeStorage(sid);
                        Trace.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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

        private async void SubmitRegisterProductExtrinsicAsync()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var ser = PolkadotApi.Serializer;

                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                        var encodedExtrinsic = ser.Serialize(new RegisterProductCall(
                            new Identifier("00012345678905"), pub,
                          //Empty.Instance
                          //OneOf.OneOf<Empty, ProductPropertyList>.FromT0(Empty.Instance)
                            new ProductPropertyList(new[] {
                                new ProductProperty(
                                    new Identifier("name"),
                                    new Identifier("Chocolate frog")
                                )
                            })
                          )
                        );
                        Trace.WriteLine(encodedExtrinsic.ToPrefixedHexString());

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SubmitAndSubcribeExtrinsic(encodedExtrinsic, "ProductRegistry", "register_product", sender, secret, str => tcs.SetResult(str));
                        Trace.WriteLine(sid);

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                        PolkadotApi.UnsubscribeStorage(sid);
                        Trace.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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

        private async void SubmitRegisterShipmentExtrinsicAsync()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var ser = PolkadotApi.Serializer;

                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                        var encodedExtrinsic = ser.Serialize(new RegisterShipmentCall(
                            new Identifier("S0003"), pub, new ProductIdList()));
                        Trace.WriteLine(encodedExtrinsic.ToPrefixedHexString());

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SubmitAndSubcribeExtrinsic(encodedExtrinsic, "ProductTracking", "register_shipment", sender, secret, str => tcs.SetResult(str));
                        Trace.WriteLine(sid);

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                        PolkadotApi.UnsubscribeStorage(sid);
                        Trace.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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

        private async void SubmitTrackShipmentExtrinsicAsync()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();

                _ = Task.Run(async () =>
                {
                    try
                    {
                        var ser = PolkadotApi.Serializer;

                        var sender = new Address("5GrwvaEF5zXb26Fz9rcQpDWS57CtERHpNehXCPcNoHGKutQY");
                        //var pub = AddressUtils.GetPublicKeyFromAddr(sender);
                        var secret = "0x33A6F3093F158A7109F679410BEF1A0C54168145E0CECB4DF006C1C2FFFB1F09925A225D97AA00682D6A59B95B18780C10D7032336E88F3442B42361F4A66011";

                        var shipmentId = ser.Serialize(new Identifier("S0002"));
                        var operation = Scale.EncodeCompactInteger(new BigInteger((int)ShippingOperation.Scan));
                        var timestamp = Scale.EncodeCompactInteger(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        var emptyArgs = new byte[2];
                        
                        var buf = new byte[shipmentId.Length + operation.Length + timestamp.Length + emptyArgs.Length];
                        shipmentId.CopyTo(buf.AsMemory());
                        operation.Bytes.CopyTo(buf.AsMemory(shipmentId.Length));
                        timestamp.Bytes.CopyTo(buf.AsMemory(shipmentId.Length + (int)operation.Length));
                        emptyArgs.CopyTo(buf.AsMemory(shipmentId.Length + (int)operation.Length + (int)timestamp.Length));

                        var tcs = new TaskCompletionSource<string>();
                        var sid = PolkadotApi.SubmitAndSubcribeExtrinsic(buf, "ProductTracking", "TrackShipment", sender, secret, str => tcs.SetResult(str));
                        Trace.WriteLine(sid);

                        var result = await tcs.Task.WithTimeout(TimeSpan.FromSeconds(30)).ConfigureAwait(false);
                        PolkadotApi.UnsubscribeStorage(sid);
                        Trace.WriteLine(result);
                    }
                    catch (Exception ex)
                    {
                        Trace.WriteLine(ex);
                    }
                });
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
