using Polkadot.BinarySerializer;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class RegisterShipmentCall : IExtrinsicCall
    {
        [Serialize(0)]
        public Identifier ShipmentId { get; set; }

        [Serialize(1)]
        public PublicKey Owner { get; set; }

        [Serialize(2)]
        public ProductIdList Products { get; set; }

        public RegisterShipmentCall(Identifier shipmentId, PublicKey owner, ProductIdList products)
        {
            ShipmentId = shipmentId;
            Owner = owner;
            Products = products;
        }
    }
}
