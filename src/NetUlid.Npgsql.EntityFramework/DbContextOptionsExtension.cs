namespace NetUlid.Npgsql.EntityFramework
{
    using System.Collections.Generic;
    using Microsoft.EntityFrameworkCore.Infrastructure;
    using Microsoft.Extensions.DependencyInjection;

    internal sealed class DbContextOptionsExtension : IDbContextOptionsExtension
    {
        public DbContextOptionsExtension()
        {
            this.Info = new ExtensionInfo(this);
        }

        public DbContextOptionsExtensionInfo Info { get; }

        public void ApplyServices(IServiceCollection services)
        {
            services.AddEntityFrameworkNpgsqlNetUlid();
        }

        public void Validate(IDbContextOptions options)
        {
        }

        private sealed class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override bool IsDatabaseProvider => false;

            public override string LogFragment => "using NetUlid ";

            public override long GetServiceProviderHashCode() => 0;

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
                debugInfo["Npgsql:UseNetUlid"] = "1";
            }
        }
    }
}
