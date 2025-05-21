using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyAssistant.Core;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services;
using PolyAssistant.Core.Services.Interfaces;
using PolyAssistant.Desktop.Components.Interfaces;
using PolyAssistant.Desktop.Services;
using PolyAssistant.Desktop.Services.Interfaces;
using PolyAssistant.Desktop.ViewModels;
using Serilog;

namespace PolyAssistant.Desktop;

public partial class App : IApplicationServiceProvider
{
    public App()
    {
        var configuration =
            new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", reloadOnChange: true, optional: false)
                .AddJsonFile("appsettings.user.json", reloadOnChange: true, optional: true)
                .AddEnvironmentVariables()
                .Build();

        var services =
            new ServiceCollection()
                // configuration
                .Configure<OllamaConfiguration>(configuration.GetSection(OllamaConfiguration.Key))
                // services
                .AddSingleton<IConfiguration>(configuration)
                .AddSingleton<MainViewModel>()
                .AddScoped<IAgentService, AgentService>()
                .AddScoped<IAudioService, AudioService>()
                .AddScoped<IHotkeyService, HotkeyService>()
                .AddScoped<IAutomationService, AutomationService>()
                .AddPolyAssistantApiServicesScoped(configuration)
                // logging
                .AddSerilog(x => x.ReadFrom.Configuration(configuration));

        if (configuration["Ollama:Url"]?.StartsWith("https", StringComparison.OrdinalIgnoreCase) == true)
        {
            services.AddInsecureHttpsClient();
        }
        else
        {
            services.AddHttpClient();
        }

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceProvider ServiceProvider { get; }
}