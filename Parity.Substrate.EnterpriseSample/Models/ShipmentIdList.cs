using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ShipmentIdList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public Identifier[] ShipmentIds { get; set; }

        public ShipmentIdList()
        {
            ShipmentIds = new Identifier[0];
        }
    }
}
