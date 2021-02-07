using System;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;
using Polkadot.DataStructs;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class OrganizationList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public PublicKey[] Organizations { get; set; }

        public OrganizationList()
        {
            Organizations = new PublicKey[0];
        }

        internal object FirstOrDefault()
        {
            throw new NotImplementedException();
        }
    }
}
