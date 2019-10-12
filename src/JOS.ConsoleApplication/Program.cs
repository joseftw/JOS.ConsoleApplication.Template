using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JOS.ConsoleApplication.Core;
using JOS.ConsoleApplication.Infrastructure.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace JOS.ConsoleApplication
{
    public class Program
    {
        private static readonly IList<(string Message, LogLevel Level)> ApplicationInformationMessages = new List<(string, LogLevel)>();

        public static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder().AddCommandLine(args).Build();
            var environment = GetEnvironment(configuration);
            
            var builder = new HostBuilder()
                .UseEnvironment(environment)
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddConfiguration(configuration);
                    config.AddJsonFile("appsettings.json");
                    config.AddJsonFile($"appsettings.{hostingContext.HostingEnvironment}.json", optional: true);
                    config.AddJsonFile("appsettings.Local.json", optional: true);
                    config.AddEnvironmentVariables();
                    config.AddCommandLine(args);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IConfigurator, LoggerConfigurator>();
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddSerilog(dispose: true);
                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateOnBuild = true;
                    options.ValidateScopes = true;
                });

            var host = builder.Build();

            await RunConfigurators(host.Services);

            LogApplicationInformation(host.Services);
            await host.RunAsync();
        }

        private static string GetEnvironment(IConfiguration configuration)
        {
            var environment = configuration.GetValue<string>("environment");

            if (string.IsNullOrWhiteSpace(environment))
            {
                ApplicationInformationMessages.Add(($"No environment supplied, defaulting to {Environments.Development}", LogLevel.Warning));
                environment = Environments.Development;
            }

            return environment;
        }
        
        private static void LogApplicationInformation(IServiceProvider serviceProvider)
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            foreach (var message in ApplicationInformationMessages)
            {
                logger.Log(message.Level, message.Message);
            }

            var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
            logger.LogInformation($"Starting application. Environment: {hostEnvironment.EnvironmentName}");
        }

        private static async Task RunConfigurators(IServiceProvider serviceProvider)
        {
            var configurators = serviceProvider.GetServices<IConfigurator>();

            foreach (var configurator in configurators)
            {
                await configurator.Configure();
            }
        }
    }
}
