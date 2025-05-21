using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Text;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public class TextController(IOllamaService ollamaService, IWhisperService whisperService) : ControllerBase
{
    /// <summary>
    ///     Extract text from images, PDFs, etc.
    /// </summary>
    [HttpPost, Route("ocr")]
    public async Task<IActionResult> OcrAsync()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Extract text from audio.
    /// </summary>
    [HttpPost, Route("transcribe")]
    public async Task<IActionResult> TranscribeAsync(IFormFile wavFile)
    {
        var wavData = await wavFile.ToBytesAsync();
        var result = await whisperService.TranscribeAsync(wavData);

        return Ok(result);
    }

    /// <summary>
    ///     Translate text.
    /// </summary>
    [HttpPost, Route("translate")]
    public async Task<IActionResult> TranslateTextAsync([FromBody] TextTranslationQueryModel query)
    {
        var text = query.Text;

        if (string.IsNullOrWhiteSpace(text))
        {
            return NoContent();
        }

        var to = query.To;

        if (string.IsNullOrWhiteSpace(to))
        {
            return BadRequest("\"to\" must be specified");
        }

        var from = query.From;
        var consistency = query.Consistency;

        if (consistency < 0)
        {
            consistency = 0;
        }

        if (consistency > 1)
        {
            consistency = 1;
        }

        var message = string.IsNullOrWhiteSpace(from)
            ? $"Translate the provided text to \"{to}\":{Environment.NewLine}{Environment.NewLine}{text}"
            : $"Translate the provided text from \"{from}\" to \"{to}\":{Environment.NewLine}{Environment.NewLine}{text}";

        const string systemMessage = "You are a translation machine. You translate text from a language to another language. Only respond with the translated text. Nothing else. Do not embellish your response.";

        var settings = new OpenAIPromptExecutionSettings
        {
            TopP = 1.0 - consistency
        };

        var result = await ollamaService.ChatAsync(message, systemMessage, null, null, settings, null, true);

        if (string.IsNullOrWhiteSpace(result.Message))
        {
            return NoContent();
        }

        return Ok(result.Message);
    }
}