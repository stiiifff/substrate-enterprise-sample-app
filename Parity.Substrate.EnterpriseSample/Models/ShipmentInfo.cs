using System;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ShipmentInfo
    {
        public string ShipmentId { get; set; }
        public string Owner { get; set; }
        public string Status { get; set; }
        public ProductInfo[] Products { get; set; }
        public DateTime? Registered { get; set; }
        public DateTime? Delivered { get; set; }
    }
}
