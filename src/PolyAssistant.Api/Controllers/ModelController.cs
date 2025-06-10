using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Model;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ModelController(IOllamaService ollamaService) : ControllerBase
{
    [HttpPost]
    [Route("create")]
    public async Task<IActionResult> CreateModelAsync([FromBody] ModelCreateRequestModel request)
    {
        var path = request.Path;
        var modelfile = request.Modelfile.ToModelfileContent();

        await ollamaService.CreateModelAsync(path, modelfile);

        return Ok();
    }

    [HttpPost]
    [Route("download")]
    public async Task<IActionResult> DownloadModelAsync([FromBody] ModelDownloadRequestModel request)
    {
        var url = request.Url;
        var path = request.Path;

        await ollamaService.DownloadModelAsync(url, path);

        return Ok();
    }
}