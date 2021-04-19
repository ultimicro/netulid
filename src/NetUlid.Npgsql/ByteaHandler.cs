namespace NetUlid.Npgsql
{
    using System;
    using System.Buffers;
    using System.Threading;
    using System.Threading.Tasks;
    using global::Npgsql.TypeHandling;
    using BaseHanler = global::Npgsql.TypeHandlers.ByteaHandler;

    internal sealed class ByteaHandler :
        NpgsqlTypeHandler<Ulid>,
        INpgsqlTypeHandler<byte[]>,
        INpgsqlTypeHandler<ArraySegment<byte>>,
        INpgsqlTypeHandler<ReadOnlyMemory<byte>>,
        INpgsqlTypeHandler<Memory<byte>>
    {
        private readonly BaseHanler @base;

        public ByteaHandler(global::Npgsql.PostgresTypes.PostgresType postgresType)
            : base(postgresType)
        {
            this.@base = new BaseHanler(postgresType);
        }

        public override async ValueTask<Ulid> Read(
            global::Npgsql.NpgsqlReadBuffer buf,
            int len,
            bool async,
            global::Npgsql.BackendMessages.FieldDescription? fieldDescription = null)
        {
            // Make sure the data has correct length.
            if (len != 16)
            {
                throw new InvalidCastException($"Cannot read bytea with length {len} as {typeof(Ulid)}.");
            }

            await buf.Ensure(len, async);

            // Read.
            using var data = MemoryPool<byte>.Shared.Rent(16);

            buf.ReadBytes(data.Memory.Span.Slice(0, 16));

            return new Ulid(data.Memory.Span.Slice(0, 16));
        }

        public override int ValidateAndGetLength(
            Ulid value,
            ref global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter)
        {
            return 16;
        }

        public override async Task Write(
            Ulid value,
            global::Npgsql.NpgsqlWriteBuffer buf,
            global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft >= 16)
            {
                using var data = MemoryPool<byte>.Shared.Rent(16);

                value.Write(data.Memory.Span);

                buf.WriteBytes(data.Memory.Span.Slice(0, 16));
            }
            else
            {
                await buf.WriteBytesRaw(value.ToByteArray(), async, cancellationToken);
            }
        }

        ValueTask<byte[]> INpgsqlTypeHandler<byte[]>.Read(
            global::Npgsql.NpgsqlReadBuffer buf,
            int len,
            bool async,
            global::Npgsql.BackendMessages.FieldDescription? fieldDescription)
        {
            return this.@base.Read(buf, len, async, fieldDescription);
        }

        int INpgsqlTypeHandler<byte[]>.ValidateAndGetLength(byte[] value, ref global::Npgsql.NpgsqlLengthCache? lengthCache, global::Npgsql.NpgsqlParameter? parameter)
        {
            return this.@base.ValidateAndGetLength(value, ref lengthCache, parameter);
        }

        Task INpgsqlTypeHandler<byte[]>.Write(
            byte[] value,
            global::Npgsql.NpgsqlWriteBuffer buf,
            global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter,
            bool async,
            CancellationToken cancellationToken)
        {
            return this.@base.Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        ValueTask<ArraySegment<byte>> INpgsqlTypeHandler<ArraySegment<byte>>.Read(
            global::Npgsql.NpgsqlReadBuffer buf,
            int len,
            bool async,
            global::Npgsql.BackendMessages.FieldDescription? fieldDescription)
        {
            return ((INpgsqlTypeHandler<ArraySegment<byte>>)this.@base).Read(buf, len, async, fieldDescription);
        }

        int INpgsqlTypeHandler<ArraySegment<byte>>.ValidateAndGetLength(
            ArraySegment<byte> value,
            ref global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter)
        {
            return ((INpgsqlTypeHandler<ArraySegment<byte>>)this.@base).ValidateAndGetLength(value, ref lengthCache, parameter);
        }

        Task INpgsqlTypeHandler<ArraySegment<byte>>.Write(
            ArraySegment<byte> value,
            global::Npgsql.NpgsqlWriteBuffer buf,
            global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter,
            bool async,
            CancellationToken cancellationToken)
        {
            return ((INpgsqlTypeHandler<ArraySegment<byte>>)this.@base).Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        ValueTask<ReadOnlyMemory<byte>> INpgsqlTypeHandler<ReadOnlyMemory<byte>>.Read(
            global::Npgsql.NpgsqlReadBuffer buf,
            int len,
            bool async,
            global::Npgsql.BackendMessages.FieldDescription? fieldDescription)
        {
            return ((INpgsqlTypeHandler<ReadOnlyMemory<byte>>)this.@base).Read(buf, len, async, fieldDescription);
        }

        int INpgsqlTypeHandler<ReadOnlyMemory<byte>>.ValidateAndGetLength(
            ReadOnlyMemory<byte> value,
            ref global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter)
        {
            return ((INpgsqlTypeHandler<ReadOnlyMemory<byte>>)this.@base).ValidateAndGetLength(value, ref lengthCache, parameter);
        }

        Task INpgsqlTypeHandler<ReadOnlyMemory<byte>>.Write(
            ReadOnlyMemory<byte> value,
            global::Npgsql.NpgsqlWriteBuffer buf,
            global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter,
            bool async,
            CancellationToken cancellationToken)
        {
            return ((INpgsqlTypeHandler<ReadOnlyMemory<byte>>)this.@base).Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        ValueTask<Memory<byte>> INpgsqlTypeHandler<Memory<byte>>.Read(
            global::Npgsql.NpgsqlReadBuffer buf,
            int len,
            bool async,
            global::Npgsql.BackendMessages.FieldDescription? fieldDescription)
        {
            return ((INpgsqlTypeHandler<Memory<byte>>)this.@base).Read(buf, len, async, fieldDescription);
        }

        int INpgsqlTypeHandler<Memory<byte>>.ValidateAndGetLength(
            Memory<byte> value,
            ref global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter)
        {
            return ((INpgsqlTypeHandler<Memory<byte>>)this.@base).ValidateAndGetLength(value, ref lengthCache, parameter);
        }

        Task INpgsqlTypeHandler<Memory<byte>>.Write(
            Memory<byte> value,
            global::Npgsql.NpgsqlWriteBuffer buf,
            global::Npgsql.NpgsqlLengthCache? lengthCache,
            global::Npgsql.NpgsqlParameter? parameter,
            bool async,
            CancellationToken cancellationToken)
        {
            return ((INpgsqlTypeHandler<Memory<byte>>)this.@base).Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }
    }
}
