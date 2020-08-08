using System.Numerics;
using Polkadot.BinarySerializer;
using Polkadot.BinarySerializer.Converters;

namespace Parity.Substrate.EnterpriseSample.Models
{
    public class TrackShipmentCall : IExtrinsicCall
    {
        [Serialize(0)]
        public Identifier ShipmentId { get; set; }

        [Serialize(1)]
        public byte Operation { get; set; }

        [Serialize(2)]
        //[CompactBigIntegerConverter]
        public long Timestamp { get; set; }

        [Serialize(3)]
        public ReadPoint Location { get; set; }

        [Serialize(4)]
        public ReadingList Readings { get; set; }

        public TrackShipmentCall(Identifier shipmentId,
            byte operation, long timestamp,
            ReadPoint location, ReadingList readings)
        {
            ShipmentId = shipmentId;
            Operation = operation;
            Timestamp = timestamp;
            Location = location;
            Readings = readings;
        }
    }
}
