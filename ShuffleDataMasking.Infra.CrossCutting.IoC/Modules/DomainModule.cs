using ShuffleDataMasking.Domain.Masking.Interfaces.Services;
using ShuffleDataMasking.Domain.Masking.Services;
using Microsoft.Extensions.DependencyInjection;

namespace ShuffleDataMasking.Infra.CrossCutting.IoC.Modules
{
    public static class DomainModule
    {
        public static void Register(IServiceCollection services)
        {
            services.AddScoped<IReductionService, ReductionService>();
            services.AddScoped<IForeignKeysService, ForeignKeysService>();
            services.AddScoped<IMaskGeneratorService, MaskGeneratorService>();
            services.AddScoped<IProcessErrorService, ProcessErrorService>();
            services.AddScoped<IShuffleDataMaskingService, ShuffleDataMaskingService>();
        }
    }
}

