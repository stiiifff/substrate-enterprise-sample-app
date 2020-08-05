using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ProductIdList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public Identifier[] ProductIds;

        public ProductIdList()
        {
            ProductIds = new Identifier[0];
        }
    }
}
