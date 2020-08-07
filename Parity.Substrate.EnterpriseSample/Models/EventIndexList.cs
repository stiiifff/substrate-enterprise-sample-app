using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class EventIndex
    {
        [Serialize(0)]
        [FixedSizeArrayConverter(16)]
        public byte[] Value { get; set; }
    }

    public class EventIndexList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public EventIndex[] EventIndices { get; set; }
    }
}
