namespace Microsoft.Extensions.DependencyInjection
{
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.EntityFrameworkCore.Storage;
    using NetUlid.Npgsql.EntityFramework;

    public static class ServiceCollectionExtensions
    {
        public static void AddEntityFrameworkNpgsqlNetUlid(this IServiceCollection services)
        {
            var builder = new EntityFrameworkRelationalServicesBuilder(services);

            builder.TryAddProviderSpecificServices(services =>
            {
                services.TryAddSingletonEnumerable<IRelationalTypeMappingSourcePlugin, RelationalTypeMappingSourcePlugin>();
            });
        }
    }
}
