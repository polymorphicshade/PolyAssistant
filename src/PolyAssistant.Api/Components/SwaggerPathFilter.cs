using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PolyAssistant.Api.Components;

public class SwaggerPathFilter(string[]? keyFilterPatterns = null) : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        if (keyFilterPatterns == null)
        {
            return;
        }

        foreach (var key in swaggerDoc.Paths.Keys.ToArray())
        {
            foreach (var item in keyFilterPatterns)
            {
                if (Regex.IsMatch(key, item, RegexOptions.IgnoreCase))
                {
                    swaggerDoc.Paths.Remove(key);
                }
            }
        }
    }
}