using System.Numerics;
using OneOf;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ShipmentStatus
    {
        public class Pending
        {
            public override string ToString() => "Pending";
        }
        public class InTransit
        {
            public override string ToString() => "InTransit";
        }
        public class Delivered
        {
            public override string ToString() => "Delivered";
        }
    }

    public class Shipment
    {
        [Serialize(0)]
        public Identifier Id { get; set; }

        [Serialize(1)]
        public PublicKey Owner { get; set; }

        [Serialize(2)]
        [OneOfConverter]
        public OneOf<ShipmentStatus.Pending,
            ShipmentStatus.InTransit,
            ShipmentStatus.Delivered> Status { get; set; }

        [Serialize(3)]
        public ProductIdList Products { get; set; }

        [Serialize(4)]
        [CompactBigIntegerConverter]
        public BigInteger Registered { get; set; }

        //[Serialize(5)]
        //[OneOfConverter]
        //[CompactBigIntegerConverter]
        //public OneOf<Empty, BigInteger> Delivered { get; set; }
    }
}
