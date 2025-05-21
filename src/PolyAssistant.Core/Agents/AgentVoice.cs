using PolyAssistant.Core.Agents.Interfaces;

namespace PolyAssistant.Core.Agents;

public abstract class AgentVoice : IAgentVoice
{
    public abstract string VoiceFilePath { get; }
}