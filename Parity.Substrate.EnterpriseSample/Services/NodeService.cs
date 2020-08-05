using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Parity.Substrate.EnterpriseSample.Models;
using Polkadot.Api;
using Prism.Events;

namespace Parity.Substrate.EnterpriseSample.Services
{
    public class NodeService : INodeService, IDisposable
    {
        string blockSid;
        readonly SubscriptionToken eventSubs;
        readonly BehaviorSubject<long> blockSubject = new BehaviorSubject<long>(0);

        public NodeService(IEventAggregator eventAggregator, IApplication polkadotApi)
        {
            EventAggregator = eventAggregator;
            PolkadotApi = polkadotApi;

            eventSubs = EventAggregator.GetEvent<ApiStatusEvent>().Subscribe(status =>
            {
                if (status == ApiStatus.ApiReady)
                {
                    if (!string.IsNullOrEmpty(blockSid))
                        PolkadotApi.UnsubscribeBlockNumber(blockSid);
                    blockSid = PolkadotApi.SubscribeBlockNumber(blockSubject.OnNext);
                }
            });
        }

        public IApplication PolkadotApi { get; }
        public IEventAggregator EventAggregator { get; }
        public IObservable<long> BestBlock => blockSubject.AsObservable();

        public void Dispose()
        {
            if (eventSubs != null)
                EventAggregator.GetEvent<ApiStatusEvent>().Unsubscribe(eventSubs);
            if (!string.IsNullOrEmpty(blockSid))
                PolkadotApi.UnsubscribeBlockNumber(blockSid);
            ((IDisposable)blockSubject).Dispose();
        }
    }
}
