namespace PolyAssistant.Core.Agents.Interfaces;

public interface IZonosAgentVoice : IAgentVoice
{
    string Language { get; }

    float Happiness { get; }

    float Sadness { get; }

    float Disgust { get; }

    float Fear { get; }

    float Surprise { get; }

    float Anger { get; }

    float Other { get; }

    float Neutral { get; }

    float PitchStd { get; }

    float SpeakingRate { get; }
}