using ShuffleDataMasking.Infra.CrossCutting.IoC;
using ShuffleDataMasking.Infra.CrossCutting.IoC.Modules;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

namespace ShuffleDataMasking.Worker
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var env = ReturnEnvironment(config);
                    config
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile($"appsettings.{(string.IsNullOrEmpty(env) ? "Staging" : env)}.json", optional: true, reloadOnChange: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.ConfigureContainer(hostContext.Configuration);
                    services.AddHostedService<Worker>();
                })
                .UseCustomLogging();

        private static string ReturnEnvironment(IConfigurationBuilder configBuilder)
            => configBuilder.SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build()
                .GetSection("Environment")?.Value;
    }
}

