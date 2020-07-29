using System.Text;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class Identifier
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public byte[] Bytes;

        public Identifier() { }

        public Identifier(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new System.ArgumentException("message", nameof(id));
            }
            Bytes = Encoding.UTF8.GetBytes(id);
        }

        public override string ToString()
        {
            return Encoding.UTF8.GetString(Bytes);
        }
    }
}
