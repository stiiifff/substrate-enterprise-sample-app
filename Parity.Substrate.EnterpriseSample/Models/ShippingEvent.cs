using OneOf;
using Polkadot.BinaryContracts;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class ShippingEventType
    {
        public class ShipmentRegistration
        {
            public override string ToString() => "Registration";
        }
        public class ShipmentPickup
        {
            public override string ToString() => "Pickup";
        }
        public class ShipmentScan
        {
            public override string ToString() => "Scan";
        }
        public class ShipmentDeliver
        {
            public override string ToString() => "Deliver";
        }
    }

    public class ReadingType
    {
        public class Humidity
        {
            public override string ToString() => "Humidity";
        }
        public class Pressure
        {
            public override string ToString() => "Pressure";
        }
        public class Shock
        {
            public override string ToString() => "Shock";
        }
        public class Tilt
        {
            public override string ToString() => "Tilt";
        }
        public class Temperature
        {
            public override string ToString() => "Temperature";
        }
        public class Vibration
        {
            public override string ToString() => "Vibration";
        }
    }

    public class ReadPoint
    {
        [Serialize(0)]
        public int Latitude { get; set; }

        [Serialize(1)]
        public int Longitude { get; set; }
    }

    public class ReadingList
    {
        [Serialize(0)]
        [PrefixedArrayConverter]
        public Reading[] Readings { get; set; } = new Reading[0];
    }

    public class Reading
    {
        [Serialize(0)]
        public Identifier DeviceId { get; set; }
        [Serialize(1)]
        public ReadingType ReadingType { get; set; }
        [Serialize(2)]
        public long Timestamp { get; set; }
        [Serialize(3)]
        public int Value { get; set; }
    }

    public class ShippingEvent
    {
        [Serialize(0)]
        [OneOfConverter]
        public OneOf<ShippingEventType.ShipmentRegistration,
            ShippingEventType.ShipmentPickup,
            ShippingEventType.ShipmentScan,
            ShippingEventType.ShipmentDeliver> Type
        { get; set; }

        [Serialize(1)]
        public Identifier ShipmentId { get; set; }

        [Serialize(2)]
        [OneOfConverter]
        public OneOf<Empty,ReadPoint> Location { get; set; }

        [Serialize(3)]
        [OneOfConverter]
        public OneOf<Empty,ReadingList> Readings { get; set; }

        [Serialize(4)]
        public long Timestamp { get; set; }
    }
}
