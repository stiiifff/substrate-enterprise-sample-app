using System;
using System.Linq;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ProductInfo
    {
        public string ProductId { get; set; }
        public string Owner { get; set; }
        public ProductPropertyInfo[] Props { get; set; }
        public DateTime? Registered { get; set; }

        public string Name
        {
            get {
                return Props?.FirstOrDefault(p => p.Name.Equals("desc", StringComparison.InvariantCultureIgnoreCase))?.Value ?? "";
            }
        }
    }
}
