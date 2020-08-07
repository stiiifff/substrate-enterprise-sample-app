using OneOf;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class Product
    {
        [Serialize(0)]
        public Identifier Id { get; set; }

        [Serialize(1)]
        public PublicKey Owner { get; set; }

        [Serialize(2)]
        [OneOfConverter]
        public OneOf<Empty,ProductPropertyList> PropList { get; set; }

        [Serialize(3)]
        public long Registered { get; set; }
    }
}
