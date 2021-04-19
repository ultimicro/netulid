namespace Npgsql
{
    using System;
    using System.Data;
    using NetUlid;
    using NetUlid.Npgsql;
    using Npgsql.TypeMapping;
    using NpgsqlTypes;

    public static class NpgsqlTypeMapperExtensions
    {
        public static INpgsqlTypeMapper UseNetUlid(this INpgsqlTypeMapper mapper)
        {
            var builder = new NpgsqlTypeMappingBuilder()
            {
                PgTypeName = "bytea",
                NpgsqlDbType = NpgsqlDbType.Bytea,
                DbTypes = new[] { DbType.Binary },
                ClrTypes = new[]
                {
                    typeof(Ulid),
                    typeof(byte[]),
                    typeof(ArraySegment<byte>),
                    typeof(ReadOnlyMemory<byte>),
                    typeof(Memory<byte>),
                },
                InferredDbType = DbType.Binary,
                TypeHandlerFactory = new ByteaHandlerFactory(),
            };

            return mapper.AddMapping(builder.Build());
        }
    }
}
