using System;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public interface INodeService
    {
        IObservable<long> BestBlock { get; }
    }
}
