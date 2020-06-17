using System;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample
{
    public interface ILightClient
    {
        Task InitAsync();
        Task StartAsync();
        Task StopASync();
    }
}
