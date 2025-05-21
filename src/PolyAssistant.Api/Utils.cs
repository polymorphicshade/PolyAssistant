namespace PolyAssistant.Api;

public static class Utils
{
    public static string GetTimeStampedFileName(string extension)
    {
        if (!extension.StartsWith('.'))
        {
            extension = $".{extension}";
        }

        return $"{DateTime.Now.ToString("u").Replace(" ", "_")}{extension}";
    }
}