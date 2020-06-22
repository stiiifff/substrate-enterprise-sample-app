using System;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample
{
    public interface ILightClient
    {
        bool IsInitialized { get; }
        bool IsRunning { get; }

        Task InitAsync();
        Task StartAsync();
        Task StopASync();
    }
}
