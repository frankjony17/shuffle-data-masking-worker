using ShuffleDataMasking.Infra.CrossCutting.IoC.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ShuffleDataMasking.Infra.CrossCutting.IoC
{
    public static class IoC
    {
        public static IServiceCollection ConfigureContainer(this IServiceCollection services, IConfiguration configuration)
        {
            DataModule.Register(services, configuration);
            DomainModule.Register(services);
            InfrastructureModule.Register(services);
            return services;
        }
    }
}

