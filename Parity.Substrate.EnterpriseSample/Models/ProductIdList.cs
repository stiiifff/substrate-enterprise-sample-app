using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ProductIdList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public Identifier[] ProductIds { get; set; }

        public ProductIdList()
        {
            ProductIds = new Identifier[0];
        }
    }
}
