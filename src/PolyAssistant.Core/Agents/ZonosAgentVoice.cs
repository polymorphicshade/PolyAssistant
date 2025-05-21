using PolyAssistant.Core.Agents.Interfaces;

namespace PolyAssistant.Core.Agents;

public class ZonosAgentVoice(string voiceFilePath) : AgentVoice, IZonosAgentVoice
{
    public override string VoiceFilePath { get; } = voiceFilePath;

    public virtual string Language { get; set; } = "en-us";

    public virtual float Happiness { get; set; } = 1.0f;

    public virtual float Sadness { get; set; } = 0.05f;

    public virtual float Disgust { get; set; } = 0.05f;

    public virtual float Fear { get; set; } = 0.05f;

    public virtual float Surprise { get; set; } = 0.05f;

    public virtual float Anger { get; set; } = 0.05f;

    public virtual float Other { get; set; } = 0.1f;

    public virtual float Neutral { get; set; } = 0.2f;

    public virtual float PitchStd { get; set; } = 45.0f;

    public virtual float SpeakingRate { get; set; } = 15.0f;
}