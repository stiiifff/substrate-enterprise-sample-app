﻿using System;
using System.Linq;
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
        [CompactBigIntegerConverter]
        public BigInteger Timestamp { get; set; }

        [Serialize(3)]
        public byte Location { get; set; }

        [Serialize(4)]
        public byte Readings { get; set; }

        public TrackShipmentCall(Identifier shipmentId,
            int operation, long timestamp,
            ReadPoint location,
            ReadingList readings)
        {
            ShipmentId = shipmentId;
            // SCALE-encoded enum (int bytes are LE on ARM hence First)
            Operation = BitConverter.GetBytes(operation).First();
            Timestamp = new BigInteger(timestamp);
            //TODO: ignore location & readings args for now,
            // there is an issue with SCALE-encoding OneOf<Empty,T>,
            // so we pass empty option (1 zero-byte) for now.
            Location = new byte();
            Readings = new byte();
        }
    }
}
