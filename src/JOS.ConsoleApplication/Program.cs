using System.Threading.Tasks;
using JOS.ConsoleApplication.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JOS.ConsoleApplication
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();

                    if (args != null)
                    {
                        config.AddCommandLine(args);
                    }
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.Configure<DaemonConfig>(hostContext.Configuration.GetSection("Daemon"));
                    services.AddSingleton<IHostedService, Daemon>();
                })
                .ConfigureLogging((hostingContext, logging) => {
                    logging.AddConsole();
                });

            await builder.RunConsoleAsync();
        }
    }
}
