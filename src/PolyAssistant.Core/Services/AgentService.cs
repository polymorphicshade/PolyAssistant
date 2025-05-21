using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PolyAssistant.Core.Agents.Interfaces;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class AgentService(ILogger<AgentService> logger, IOptions<AgentServiceConfiguration> agentServiceConfiguration, IServiceProvider serviceProvider, IAssemblyLoadingService assemblyLoadingService) : IAgentService
{
    public async Task<IAgent[]> GetAgentsAsync(string? directoryPath = null, CancellationToken cancellationToken = default)
    {
        directoryPath ??= agentServiceConfiguration.Value.DefaultPluginDirectoryPath;

        var result = new List<IAgent>();

        // from current domain
        var builtInTypes =
            AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SelectMany(x => x.FindTypes<IAgent>())
                // TODO: figure out why I get more than 1 of the same Type
                .DistinctBy(x => x.FullName);

        // from plugin directory
        var otherTypes =
            Directory
                .EnumerateFiles(directoryPath, "*.dll")
                .Select(assemblyLoadingService.Load)
                .SelectMany(assembly =>
                    assembly
                        .FindTypes<IAgent>()
                        // TODO: figure out why I get more than 1 of the same Type
                        .DistinctBy(type => type.FullName));

        foreach (var type in builtInTypes.Concat(otherTypes))
        {
            try
            {
                var agent = BuildAgent<IAgent>(type);

                // notify
                await agent.OnLoadedAsync(cancellationToken);

                await agent.InitializeAsync(cancellationToken);

                logger.LogInformation("Loaded agent: \"{name}\" ({type})", agent.Name, type.FullName);

                result.Add(agent);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to load agent type: {type}", type.FullName);
            }
        }

        return result.ToArray();
    }

    private T BuildAgent<T>(Type type) where T : IAgent
    {
        var constructor = type.GetConstructor([typeof(ILogger<>).MakeGenericType(type), typeof(IServiceProvider)]);
        if (constructor == null)
        {
            constructor = type.GetConstructor([typeof(IServiceProvider)]);
            if (constructor == null)
            {
                throw new InvalidOperationException($"No suitable constructor found for type '{type.Name}'. Expected constructors with (ILogger<{type.Name}> logger, IServiceProvider serviceProvider) or (IServiceProvider serviceProvider).");
            }
        }

        object[] parameters;
        if (constructor.GetParameters().Length == 2)
        {
            // We need to create an ILogger<T> instance using the application's service provider
            var loggerType = typeof(ILogger<>).MakeGenericType(type);
            var agentLogger = serviceProvider.GetRequiredService(loggerType);
            parameters = [agentLogger, serviceProvider];
        }
        else
        {
            throw new InvalidOperationException("Unexpected number of ctor parameters");
        }

        var result = Activator.CreateInstance(type, parameters) ?? throw new ArgumentException($"Failed to activate type \"{type.FullName}\"");

        return (T)result;
    }
}