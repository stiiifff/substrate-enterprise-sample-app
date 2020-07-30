using System;
using System.Numerics;
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
        public byte Props { get; set; }

        public RegisterProductCall()
        {
        }

        public RegisterProductCall(Identifier productId, PublicKey owner)
        {
            ProductId = productId;
            Owner = owner;
            Props = new byte();
        }
    }
}
