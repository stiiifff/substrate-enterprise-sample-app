using Prism.Events;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public enum NodeStatus
    {
        NodeInitialized,
        NodeReady,
        NodeError
    }

    public class NodeStatusEvent : PubSubEvent<NodeStatus> {}
}
