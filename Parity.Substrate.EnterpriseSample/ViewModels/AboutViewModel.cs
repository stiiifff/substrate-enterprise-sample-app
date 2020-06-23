using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Polkadot.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Xamarin.Forms;

namespace Parity.Substrate.EnterpriseSample.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        public AboutViewModel()
        {
            Title = "About";
            ApplicationVersion = $"{Xamarin.Essentials.AppInfo.Name} v{Xamarin.Essentials.AppInfo.Version}";
        }

        private string applicationVersion;
        public string ApplicationVersion
        {
            get { return applicationVersion; }
            set { SetProperty(ref applicationVersion, value); }
        }

        private PeersInfo peersInfo;
        public PeersInfo PeersInfo
        {
            get { return peersInfo; }
            set { SetProperty(ref peersInfo, value); }
        }

        private SystemInfo systemInfo;
        public SystemInfo SystemInfo
        {
            get { return systemInfo; }
            set { SetProperty(ref systemInfo, value); }
        }

        internal void LoadData()
        {
            IsBusy = true;
            try
            {
                if (!App.IsPolkadotApiConnected)
                    App.ConnectToNode();
                SystemInfo = GetSystemInfo();
                PeersInfo = GetSystemPeers();
                Device.StartTimer(TimeSpan.FromSeconds(10), RefreshPeers);
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
            }
            finally
            {
                IsBusy = false;
            }
        }

        private bool RefreshPeers()
        {
            try
            {
                PeersInfo = GetSystemPeers();
                return true;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.ToString());
                return false;
            }
        }

        public SystemInfo GetSystemInfo()
        {
            var systemNameQuery = JObject.FromObject(new { method = "system_name", @params = new JArray { } });
            var systemNameJson = PolkadotApi.Request(systemNameQuery);

            var systemChainQuery = new JObject { { "method", "system_chain" }, { "params", new JArray { } } };
            var systemChainJson = PolkadotApi.Request(systemChainQuery);

            var systemVersionQuery = new JObject { { "method", "system_version" }, { "params", new JArray { } } };
            var systemVersionJson = PolkadotApi.Request(systemVersionQuery);

            return new SystemInfo
            {
                ChainName = systemNameJson.Value<string>("result"),
                ChainId = systemChainJson.Value<string>("result"),
                Version = systemVersionJson.Value<string>("result"),
            };
        }

        public PeersInfo GetSystemPeers()
        {
            JObject query = new JObject { { "method", "system_peers" },
                                          { "params", new JArray { } } };
            JObject response = PolkadotApi.Request(query);

            var peers = JsonConvert.DeserializeObject<List<PeerInfo>>(response["result"].ToString());

            return new PeersInfo
            {
                Count = peers.Count,
                Peers = peers.ToArray()
            };
        }
    }
}