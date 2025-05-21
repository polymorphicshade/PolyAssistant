using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PolyAssistant.Core.Configuration;
using PolyAssistant.Core.Data.Chat;
using PolyAssistant.Core.Models.Chat;
using PolyAssistant.Core.Services;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core;

public static class Extensions
{
    public static IServiceCollection AddPolyAssistantApiServicesScoped(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return
            serviceCollection
                // configuration
                .Configure<PolyAssistantConfiguration>(configuration.GetSection(PolyAssistantConfiguration.Key))
                // services
                .AddScoped<IRemoteChatClientService, RemoteChatClientService>()
                .AddScoped<IRemoteFilesClientService, RemoteFilesClientService>()
                .AddScoped<IRemoteImageClientService, RemoteImageClientService>()
                .AddScoped<IRemoteTextClientService, RemoteTextClientService>()
                .AddScoped<IRemoteTimeClientService, RemoteTimeClientService>()
                .AddScoped<IRemoteVoiceClientService, RemoteVoiceClientService>()
                .AddScoped<IAssemblyLoadingService, AssemblyLoadingService>();
    }

    public static IServiceCollection AddPolyAssistantCoreServicesScoped(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        return
            serviceCollection
                // configuration
                .Configure<WhisperConfiguration>(configuration.GetSection(WhisperConfiguration.Key))
                .Configure<ZonosConfiguration>(configuration.GetSection(ZonosConfiguration.Key))
                .Configure<OllamaConfiguration>(configuration.GetSection(OllamaConfiguration.Key))
                .Configure<FileManagementConfiguration>(configuration.GetSection(FileManagementConfiguration.Key))
                .Configure<CachingConfiguration>(configuration.GetSection(CachingConfiguration.Key))
                .Configure<SearXngConfiguration>(configuration.GetSection(SearXngConfiguration.Key))
                .Configure<Crawl4AiConfiguration>(configuration.GetSection(Crawl4AiConfiguration.Key))
                .Configure<AgentServiceConfiguration>(configuration.GetSection(AgentServiceConfiguration.Key))
                .Configure<ChatterboxConfiguration>(configuration.GetSection(ChatterboxConfiguration.Key))
                // services
                .AddScoped<IWhisperService, WhisperService>()
                .AddScoped<IZonosService, ZonosService>()
                .AddScoped<IOllamaService, OllamaService>()
                .AddScoped<IFileSystemService, FileSystemService>()
                .AddScoped<ISearchService, SearchService>()
                .AddScoped<IScrapeService, ScrapeService>()
                .AddScoped<IChatterboxService, ChatterboxService>();
    }

    public static IServiceCollection AddHttpsClient(this IServiceCollection serviceCollection, string? name = null, Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool>? serverCertificateCustomValidationCallback = null)
    {
        name ??= string.Empty;

        serviceCollection
            .AddHttpClient(name)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = serverCertificateCustomValidationCallback
            });

        return serviceCollection;
    }

    public static IServiceCollection AddInsecureHttpsClient(this IServiceCollection serviceCollection, string? name = null)
    {
        return serviceCollection.AddHttpsClient(name, (_, _, _, _) => true);
    }

    public static async Task<byte[]> ToBytesAsync(this IFormFile file)
    {
        using var memoryStream = new MemoryStream();
        await file.CopyToAsync(memoryStream);

        return memoryStream.ToArray();
    }

    public static bool RepresentsMissingModel(this Exception exception, string model)
    {
        return exception.Message.Contains(model) && exception.Message.Contains("not found", StringComparison.OrdinalIgnoreCase);
    }

    public static ChatMessageModel ToModel(this ChatMessageEntity entity)
    {
        return new ChatMessageModel
        {
            Id = entity.Id,
            TimeUtc = entity.TimeUtc,
            Content = entity.Content,
            Role = entity.Role
        };
    }

    public static ChatConversationModel ToModel(this ChatConversationEntity entity)
    {
        return new ChatConversationModel
        {
            Id = entity.Id,
            TimeUtc = entity.TimeUtc,
            Messages =
                entity
                    .Messages
                    .OrderBy(x => x.TimeUtc)
                    .Select(x => x.ToModel())
                    .ToArray()
        };
    }

    public static DateTime ToDateTime(this double unixTimestamp)
    {
        var dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return dateTime.AddMilliseconds(unixTimestamp);
    }

    public static DateTime ToDateTime(this string str)
    {
        return DateTime.Parse(str);
    }

    public static long GetUtcMilliseconds(this DateTime target)
    {
        var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return (long)(target - epoch).TotalMilliseconds;
    }

    public static string ToNormalizedPluginName(this string str)
    {
        return str.Replace(' ', '_').Trim();
    }

    public static IEnumerable<Type> FindTypes<T>(this Assembly assembly, string? pattern = null)
    {
        var type = typeof(T);

        var result =
            assembly
                .GetLoadedReferencedAssemblies()
                .SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                })
                .Where(x => type.IsAssignableFrom(x) && x is
                {
                    IsClass: true,
                    IsAbstract: false
                });

        result = string.IsNullOrWhiteSpace(pattern)
            ? result
            : result.Where(x => !string.IsNullOrWhiteSpace(x.FullName)
                                && Regex.IsMatch(x.FullName, pattern, RegexOptions.IgnoreCase));

        return result;
    }

    public static IEnumerable<Type> FindTypes(this Assembly assembly, string? pattern = null)
    {
        var result =
            assembly
                .GetLoadedReferencedAssemblies()
                .SelectMany(x =>
                {
                    try
                    {
                        return x.GetTypes();
                    }
                    catch (Exception)
                    {
                        return [];
                    }
                });

        result = string.IsNullOrWhiteSpace(pattern)
            ? result
            : result.Where(x => !string.IsNullOrWhiteSpace(x.FullName)
                                && Regex.IsMatch(x.FullName, pattern, RegexOptions.IgnoreCase));

        return result;
    }

    public static ParameterInfo[] GetConstructorParameters(this Type type, Type? targetAttributeType = null)
    {
        var constructors = type.GetConstructors();

        var constructor =
            (constructors
                 .FirstOrDefault(x =>
                     x.CustomAttributes.Any(y => y.AttributeType == targetAttributeType))
             ?? constructors.FirstOrDefault()) ?? throw new ArgumentException("Failed to find an applicable constructor");

        return constructor.GetParameters();
    }

    public static List<Assembly> GetLoadedReferencedAssemblies(this Assembly assembly)
    {
        var loadedAssemblies = new List<Assembly>
        {
            assembly
        };

        var referencedAssemblyNames = assembly.GetReferencedAssemblies();

        var currentDomainAssemblies =
            AppDomain
                .CurrentDomain
                .GetAssemblies()
                .ToHashSet();

        foreach (var referencedName in referencedAssemblyNames)
        {
            var loadedReferencedAssembly =
                currentDomainAssemblies
                    .FirstOrDefault(x =>
                        AssemblyName
                            .ReferenceMatchesDefinition(referencedName, x.GetName()));

            if (loadedReferencedAssembly != null && !loadedAssemblies.Contains(loadedReferencedAssembly))
            {
                loadedAssemblies.Add(loadedReferencedAssembly);
            }
        }

        return loadedAssemblies;
    }

    public static double CalculateSimilarity(this string str, string other)
    {
        if (str == other)
        {
            return 1.0;
        }

        var n = str.Length;
        var m = other.Length;

        if (n == 0)
        {
            return (m == 0) ? 1.0 : 0.0; // If s1 is empty, similarity is 0 unless s2 is also empty
        }
        if (m == 0)
        {
            return 0.0; // If s2 is also empty, similarity is 0
        }

        var d = new int[n + 1, m + 1];

        // Step 1
        for (var i = 0; i <= n; i++)
        {
            d[i, 0] = i;
        }

        for (var j = 0; j <= m; j++)
        {
            d[0, j] = j;
        }

        // Step 2
        for (var i = 1; i <= n; i++)
        {
            for (var j = 1; j <= m; j++)
            {
                // Step 3
                var cost = (other[j - 1] == str[i - 1]) ? 0 : 1;

                // Step 4
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1,      // Deletion
                        d[i, j - 1] + 1),     // Insertion
                    d[i - 1, j - 1] + cost);   // Substitution
            }
        }

        // Calculate Levenshtein distance
        var levenshteinDistance = d[n, m];

        // Normalize the distance to a similarity score
        // The maximum possible distance is the length of the longer string.
        double maxLength = Math.Max(n, m);
        if (maxLength == 0) return 1.0; // Both strings are empty

        var similarity = 1.0 - (levenshteinDistance / maxLength);

        return similarity;
    }
}