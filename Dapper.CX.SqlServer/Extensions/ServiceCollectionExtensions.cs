using Dapper.CX.SqlServer.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.CX.SqlServer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSqlServerIntCrud(this IServiceCollection services, string connectionString)
        {
            services.AddScoped((_) => new SqlServerIntCrudService(connectionString));
        }

        public static void AddSqlServerLongCrud(this IServiceCollection services, string connectionString)
        {
            services.AddScoped((_) => new SqlServerLongCrudService(connectionString));
        }
    }
}
