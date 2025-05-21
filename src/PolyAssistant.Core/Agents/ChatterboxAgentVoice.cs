using PolyAssistant.Core.Agents.Interfaces;

namespace PolyAssistant.Core.Agents;

public class ChatterboxAgentVoice(string voiceFilePath) : AgentVoice, IChatterboxAgentVoice
{
    public override string VoiceFilePath { get; } = voiceFilePath;

    public virtual double Exaggeration { get; set; } = 0.5;

    public virtual double Pace { get; set; } = 0.5f;

    public virtual double Temperature { get; set; } = 0.8f;
}