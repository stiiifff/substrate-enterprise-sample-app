using System.Numerics;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public enum ShipmentStatus
    {
        Pending,
        InTransit,
        Delivered
    }

    public class Shipment
    {
        [Serialize(0)]
        public Identifier Id { get; set; }

        [Serialize(1)]
        public PublicKey Owner { get; set; }

        [Serialize(2)]
        public ShipmentStatus Status { get; set; }

        [Serialize(3)]
        public ProductIdList Products { get; set; }

        [Serialize(4)]
        [CompactBigIntegerConverter]
        public BigInteger Registered { get; set; }

        [Serialize(5)]
        [CompactBigIntegerConverter]
        public BigInteger Delivered { get; set; }
    }
}
