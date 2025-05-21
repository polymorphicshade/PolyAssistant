using Microsoft.AspNetCore.Mvc;
using PolyAssistant.Core;
using PolyAssistant.Core.Models.Files;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Api.Controllers;

[ApiController, Route("api/[controller]")]
public sealed class FilesController(IFileSystemService fileSystemService) : Controller
{
    /// <summary>
    ///     Delete a file from the server.
    /// </summary>
    /// <param name="query">The request used for deleting files.</param>
    [HttpPost, Route("delete")]
    public IActionResult Delete([FromBody] FileDeleteQueryModel query)
    {
        var path = query.Path;

        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("Name is empty");
        }

        if (!fileSystemService.DeleteFile(path))
        {
            return NotFound($"File path not found: {path}");
        }

        return Ok();
    }

    /// <summary>
    ///     Find files that exist on the server.
    /// </summary>
    /// <param name="query">
    ///     <p>The query used for finding files.</p>
    ///     <p>
    ///         <i>The name property is used as a regex pattern.</i>
    ///     </p>
    /// </param>
    [HttpPost, Route("find")]
    public IActionResult Find([FromBody] FileQueryModel query)
    {
        if (string.IsNullOrWhiteSpace(query.Pattern))
        {
            return BadRequest("Name is empty");
        }

        var result =
            fileSystemService
                .FindFiles(query)
                .ToArray();

        return Ok(result);
    }

    /// <summary>
    ///     Upload a file to the server.
    /// </summary>
    /// <param name="file">The file to upload.</param>
    /// <param name="request">The request used for uploading files.</param>
    [HttpPost, Route("upload")]
    public async Task<IActionResult> UploadAsync(IFormFile file, [FromForm] FileUploadRequestModel request)
    {
        var path = request.Path;
        var bytes = await file.ToBytesAsync();

        if (string.IsNullOrWhiteSpace(path))
        {
            return BadRequest("FilePath is empty");
        }

        await fileSystemService.SaveFileAsync(bytes, path);

        return Ok();
    }
}