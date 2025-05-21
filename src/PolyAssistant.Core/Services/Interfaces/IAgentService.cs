using PolyAssistant.Core.Agents.Interfaces;

namespace PolyAssistant.Core.Services.Interfaces;

public interface IAgentService
{
    Task<IAgent[]> GetAgentsAsync(string? directoryPath = null, CancellationToken cancellationToken = default);
}