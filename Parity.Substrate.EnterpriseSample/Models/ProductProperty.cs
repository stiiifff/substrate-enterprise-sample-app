using Polkadot.BinarySerializer;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ProductProperty
    {
        [Serialize(0)]
        public Identifier Name { get; set; }

        [Serialize(1)]
        public Identifier Value { get; set; }

        public ProductProperty(Identifier name, Identifier value)
        {
            Name = name;
            Value = value;
        }
    }
}
