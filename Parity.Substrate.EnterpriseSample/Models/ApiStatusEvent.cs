using Prism.Events;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public enum ApiStatus
    {
        ApiReady
    }

    public class ApiStatusEvent : PubSubEvent<ApiStatus> { }
}
