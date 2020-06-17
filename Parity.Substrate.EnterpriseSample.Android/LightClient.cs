using Parity.Substrate.EnterpriseSample;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(LightClient))]

namespace Parity.Substrate.EnterpriseSample
{
    public class LightClient : ILightClient
    {
        public LightClient()
        {

        }

        public Task InitAsync()
        {
            return Task.FromResult(0);
        }

        public Task StartAsync()
        {
            return Task.FromResult(0);
        }

        public Task StopASync()
        {
            return Task.FromResult(0);
        }
    }
}