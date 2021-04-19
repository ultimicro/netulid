namespace NetUlid.Npgsql
{
    using global::Npgsql.TypeHandling;

    internal sealed class ByteaHandlerFactory : NpgsqlTypeHandlerFactory<Ulid>
    {
        public override NpgsqlTypeHandler<Ulid> Create(global::Npgsql.PostgresTypes.PostgresType pgType, global::Npgsql.NpgsqlConnection conn)
        {
            return new ByteaHandler(pgType);
        }
    }
}
