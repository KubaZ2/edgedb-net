﻿using System.Collections.Immutable;
using System.Security.Cryptography;

namespace EdgeDB.Models
{
    /// <summary>
    ///     Represents the <see href="https://www.edgedb.com/docs/reference/protocol/messages#dump-block">Dump Block</see> packet.
    /// </summary>
    public readonly struct DumpBlock : IReceiveable
    {
        /// <inheritdoc/>
        public ServerMessageType Type 
            => ServerMessageType.DumpBlock;

        /// <summary>
        ///     Gets the sha1 hash of this packets data, used when writing a dump file.
        /// </summary>
        public IReadOnlyCollection<byte> Hash
            => HashBuffer.ToImmutableArray();

        /// <summary>
        ///     Gets the length of this packets data, used when writing a dump file.
        /// </summary>
        public uint Length { get; }

        /// <summary>
        ///     Gets a collection of headers for this packet.
        /// </summary>
        public IReadOnlyCollection<Header> Headers { get; }

        internal byte[] Raw { get; }

        internal byte[] HashBuffer { get; }

        internal DumpBlock(PacketReader reader, uint length)
        {
            Length = length;

            Raw = reader.ReadBytes((int)length);

            HashBuffer = SHA1.Create().ComputeHash(Raw);

            using var r = new PacketReader(Raw);
            Headers = r.ReadHeaders();
        }
    }
}