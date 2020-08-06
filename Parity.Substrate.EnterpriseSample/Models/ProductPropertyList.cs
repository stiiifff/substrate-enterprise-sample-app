using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ProductPropertyList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public ProductProperty[] Props { get; set; }

        public ProductPropertyList(ProductProperty[] props)
        {
            Props = props;
        }
    }
}
