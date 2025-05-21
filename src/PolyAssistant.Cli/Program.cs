using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PolyAssistant.Cli.Services;
using PolyAssistant.Core;
using Serilog;

namespace PolyAssistant.Cli;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        var services = builder.Services;

        var configuration =
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
                .AddJsonFile("appsettings.user.json", reloadOnChange: true, optional: true)
                .AddEnvironmentVariables()
                .Build();

        services
            // services
            .AddHostedService<MainService>()
            .AddPolyAssistantApiServicesScoped(builder.Configuration)
            // logging
            .AddSerilog(x => x.ReadFrom.Configuration(builder.Configuration));

        if (configuration["Ollama:Url"]?.StartsWith("https", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Services.AddInsecureHttpsClient();
        }
        else
        {
            builder.Services.AddHttpClient();
        }

        var host = builder.Build();

        await host.RunAsync();
    }
}