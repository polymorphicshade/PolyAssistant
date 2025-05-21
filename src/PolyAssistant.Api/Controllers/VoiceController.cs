using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core.Models.Voice;
using PolyAssistant.Core.Services.Interfaces;

// ReSharper disable IdentifierTypo

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class VoiceController(IZonosService zonosService, IChatterboxService chatterboxService) : ControllerBase
{
    /// <summary>
    ///     Change a voice.
    /// </summary>
    [HttpPost, Route("change")]
    public Task<IActionResult> ChangeVoiceAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Generate speech from text.
    /// </summary>
    [HttpPost, Route("generate")]
    public async Task<IActionResult> GenerateSpeechAsync([FromBody] VoiceGenerationQueryModel query)
    {
        var data = await zonosService.ProcessAsync(
            query.Text,
            query.Language,
            query.VoiceFilePath,
            query.Model,
            query.PrefixAudioPath,
            query.Happiness,
            query.Sadness,
            query.Disgust,
            query.Fear,
            query.Surprise,
            query.Anger,
            query.Other,
            query.Neutral,
            query.VqScore,
            query.MaxFrequency,
            query.PitchStd,
            query.SpeakingRate,
            query.DnsmosOverall,
            query.DeNoiseSpeaker,
            query.CfgScale,
            query.SamplingTopP,
            query.SamplingTopK,
            query.SamplingMinP,
            query.SamplingLinear,
            query.SamplingConfidence,
            query.SamplingQuadratic,
            query.Seed ?? 420,
            query.Seed == null);

        return new FileContentResult(data, "application/octet-stream")
        {
            FileDownloadName = Utils.GetTimeStampedFileName(".wav")
        };
    }

    /// <summary>
    ///     Generate speech from text (usually produces better results).
    /// </summary>
    [HttpPost, Route("generate_ex")]
    public async Task<IActionResult> GenerateSpeechExAsync([FromBody] VoiceGenerationExQueryModel query)
    {
        var data = await chatterboxService.ProcessAsync(
            query.Text,
            query.VoiceFilePath,
            query.Seed,
            query.Exaggeration,
            query.Pace,
            query.Temperature);

        return new FileContentResult(data, "application/octet-stream")
        {
            FileDownloadName = Utils.GetTimeStampedFileName(".wav")
        };
    }

    [HttpPost, Route("denoise")]
    public async Task<IActionResult> DenoiseAsync()
    {
        throw new NotImplementedException();
    }
}