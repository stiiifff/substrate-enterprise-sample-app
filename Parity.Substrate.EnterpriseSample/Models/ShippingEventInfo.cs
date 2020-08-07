using System;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ReadPointInfo
    {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class ReadingInfo
    {
        public string DeviceId { get; set; }
        public string ReadingType { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Value { get; set; }
    }

    public class ShippingEventInfo
    {
        public string Type { get; set; }
        public string ShipmentId { get; set; }
        public ReadPointInfo Location { get; set; }
        public ReadingInfo[] Readings { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
