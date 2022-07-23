
using ShuffleDataMasking.Domain.Abstractions.Interfaces;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.Dapper;
using ShuffleDataMasking.Domain.Masking.Interfaces.Repositories.EntityFramework;
using ShuffleDataMasking.Infra.Data.Config;
using ShuffleDataMasking.Infra.Data.Contexts;
using ShuffleDataMasking.Infra.Data.Repositories.Dapper;
using ShuffleDataMasking.Infra.Data.Repositories.EntityFramework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using FKSolutionsSource.Infra.Logging.Debugging;

namespace ShuffleDataMasking.Infra.CrossCutting.IoC.Modules
{
    public static class DataModule
    {
        private const string FKS_CONNECTION_STRING = "FKS";
        private const string FKSOLUTIONS__CONNECTION_STRING = "FKSolutions";
        private const string SHUFFLE_DATA_MASKING_CONNECTION_STRING = "ShuffleDataMasking";
        

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<DataContext>(options => options.UseSqlServer(
                GetShuffleDataMaskingConnectionString(configuration)).UseLoggerFactory(EntityFrameworkDebugLogger.Factory));

            services.AddScoped<IDapperConnection, DapperConnection>(u => new DapperConnection(
                GetFksConnectionString(configuration),
                GetFKSolutionsConnectionString(configuration),
                GetShuffleDataMaskingConnectionString(configuration))
            );
            services.AddScoped<IIntrospectionDapperRepository, IntrospectionDapperRepository>();
            services.AddScoped<IShuffleDataMaskingDapperRepository, ShuffleDataMaskingDapperRepository>();

            services.AddScoped<IDataCollectorEfRepository, DataCollectorEfRepository>();
        }

        private static string GetFksConnectionString(IConfiguration configuration)
            => configuration.GetConnectionString(FKS_CONNECTION_STRING);

        private static string GetFKSolutionsConnectionString(IConfiguration configuration)
            => configuration.GetConnectionString(FKSOLUTIONS__CONNECTION_STRING);

        private static string GetShuffleDataMaskingConnectionString(IConfiguration configuration)
            => configuration.GetConnectionString(SHUFFLE_DATA_MASKING_CONNECTION_STRING);
    }
}

