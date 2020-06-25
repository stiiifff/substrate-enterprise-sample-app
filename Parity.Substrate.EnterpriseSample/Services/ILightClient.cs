using System;
using System.Threading.Tasks;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public interface ILightClient
    {
        bool IsInitialized { get; }
        bool IsRunning { get; }
        IObservable<string> Logs { get; }

        Task InitAsync();
        Task StartAsync();
        Task StopASync();
    }
}
