﻿using System.Collections.Immutable;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#data">Data</see> packet
    /// </summary>
    public readonly struct Data : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.Data;

        /// <summary>
        ///     Gets the payload of this data packet
        /// </summary>
        public IReadOnlyCollection<byte> PayloadData
            => PayloadData.ToImmutableArray();

        internal byte[] PayloadBuffer { get; }

        internal Data(PacketReader reader)
        {
            // skip arary since its always one, errr should be one
            var numElements = reader.ReadUInt16();
            if (numElements != 1)
            {
                throw new ArgumentOutOfRangeException(nameof(reader), $"Expected one element array for data, got {numElements}");
            }

            var payloadLength = reader.ReadUInt32();

            PayloadBuffer = reader.ReadBytes((int)payloadLength);
        }
    }
}