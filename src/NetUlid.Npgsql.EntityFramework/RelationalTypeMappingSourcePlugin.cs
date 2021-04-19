namespace NetUlid.Npgsql.EntityFramework
{
    using Microsoft.EntityFrameworkCore.Storage;

    internal sealed class RelationalTypeMappingSourcePlugin : IRelationalTypeMappingSourcePlugin
    {
        private readonly UlidTypeMapping ulidMapping;

        public RelationalTypeMappingSourcePlugin()
        {
            this.ulidMapping = new UlidTypeMapping();
        }

        public RelationalTypeMapping? FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var clrType = mappingInfo.ClrType;
            var storeType = mappingInfo.StoreTypeName;

            if (storeType != null)
            {
                if (storeType == "bytea")
                {
                    if (clrType == null || clrType == this.ulidMapping.ClrType)
                    {
                        return this.ulidMapping;
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (mappingInfo.StoreTypeNameBase == "bytea")
                {
                    if (clrType == null || clrType == this.ulidMapping.ClrType)
                    {
                        return this.ulidMapping.Clone(in mappingInfo);
                    }

                    return null;
                }
            }

            if (clrType == null || clrType != typeof(Ulid))
            {
                return null;
            }

            return this.ulidMapping;
        }
    }
}
