using OneOf;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class RegisterProductCall : IExtrinsicCall
    {
        [Serialize(0)]
        public Identifier ProductId{ get; set; }

        [Serialize(1)]
        public PublicKey Owner { get; set; }

        [Serialize(2)]
        [OneOfConverter]
        public OneOf<Empty,ProductPropertyList> Props { get; set; }

        public RegisterProductCall(Identifier productId, PublicKey owner, OneOf<Empty, ProductPropertyList> props)
        {
            ProductId = productId;
            Owner = owner;
            Props = props;
        }
    }
}
