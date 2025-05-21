namespace PolyAssistant.Core.Agents.Interfaces;

public interface IChatterboxAgentVoice : IAgentVoice
{
    double Exaggeration { get; }

    double Pace { get; }

    double Temperature { get; }
}