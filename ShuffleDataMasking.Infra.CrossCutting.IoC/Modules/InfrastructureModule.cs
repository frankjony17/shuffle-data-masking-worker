using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FKSolutionsSource.Infra.Extensions;
using FKSolutionsSource.Infra.Logging.Extensions;
using FKSolutionsSource.Infra.MessagingBroker;

namespace ShuffleDataMasking.Infra.CrossCutting.IoC.Modules
{
    public static class InfrastructureModule
    {
        public static void Register(IServiceCollection services)
        {
            services.AddFKSolutionsSourceAclServices();
            services.AddFKSolutionsSourceMessagingBroker();
        }

        public static IHostBuilder UseCustomLogging(this IHostBuilder hostBuilder, bool removeOtherLoggingProviders = true) =>
            hostBuilder.UseFKSolutionsSourceLogging(removeOtherLoggingProviders);
    }
}

