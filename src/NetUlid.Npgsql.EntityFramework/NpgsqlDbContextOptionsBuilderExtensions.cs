namespace Microsoft.EntityFrameworkCore
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using NetUlid.Npgsql.EntityFramework;
    using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

    public static class NpgsqlDbContextOptionsBuilderExtensions
    {
        public static NpgsqlDbContextOptionsBuilder UseNetUlid(this NpgsqlDbContextOptionsBuilder builder)
        {
            var core = ((IRelationalDbContextOptionsBuilderInfrastructure)builder).OptionsBuilder;
            var extension = core.Options.FindExtension<DbContextOptionsExtension>();

            if (extension == null)
            {
                extension = new DbContextOptionsExtension();
            }

            ((IDbContextOptionsBuilderInfrastructure)core).AddOrUpdateExtension(extension);

            return builder;
        }
    }
}
