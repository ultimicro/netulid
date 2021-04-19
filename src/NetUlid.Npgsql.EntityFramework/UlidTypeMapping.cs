namespace NetUlid.Npgsql.EntityFramework
{
    using System.Buffers;
    using System.Globalization;
    using System.Text;
    using global::Npgsql.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping;
    using Microsoft.EntityFrameworkCore.Storage;
    using NpgsqlTypes;

    internal sealed class UlidTypeMapping : NpgsqlTypeMapping
    {
        public UlidTypeMapping()
            : base("bytea", typeof(Ulid), NpgsqlDbType.Bytea)
        {
        }

        private UlidTypeMapping(RelationalTypeMappingParameters parameters)
            : base(parameters, NpgsqlDbType.Bytea)
        {
        }

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
        {
            return new UlidTypeMapping(parameters);
        }

        protected override string GenerateNonNullSqlLiteral(object value)
        {
            // Get binary representation.
            using var buffer = MemoryPool<byte>.Shared.Rent(16);
            var v = buffer.Memory.Slice(0, 16).Span;

            ((Ulid)value).Write(v);

            // Generate literal.
            var builder = new StringBuilder();

            builder.Append("BYTEA E'\\\\x");

            for (var i = 0; i < v.Length; i++)
            {
                builder.Append(v[i].ToString("X2", CultureInfo.InvariantCulture));
            }

            builder.Append('\'');

            return builder.ToString();
        }
    }
}
