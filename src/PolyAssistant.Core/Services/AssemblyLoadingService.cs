using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.Logging;
using PolyAssistant.Core.Services.Interfaces;

namespace PolyAssistant.Core.Services;

public sealed class AssemblyLoadingService(ILogger<AssemblyLoadingService> logger) : IAssemblyLoadingService
{
    private readonly ConcurrentDictionary<string, AssemblyLoadContext> _loadContexts = new();

    public Assembly Load(string assemblyPath)
    {
        if (!File.Exists(assemblyPath))
        {
            throw new FileNotFoundException("Assembly not found", assemblyPath);
        }

        var loadContext = _loadContexts.GetOrAdd(assemblyPath, path =>
        {
            var context = new AssemblyLoadContext(Path.GetFileNameWithoutExtension(path), true);
            context.Resolving += (ctx, assemblyName) => ResolveAssembly(ctx, assemblyName, path);
            return context;
        });

        var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);

        return assembly;
    }

    private static Assembly? ResolveAssembly(AssemblyLoadContext context, AssemblyName assemblyName, string mainAssemblyPath)
    {
        try
        {
            if (Assembly.Load(assemblyName) is { } cachedAssembly)
            {
                return cachedAssembly;
            }
        }
        catch
        {
            // ignored
        }

        var assemblyPath = Path.Combine(Path.GetDirectoryName(mainAssemblyPath)!, assemblyName.Name + ".dll");
        return File.Exists(assemblyPath) ? context.LoadFromAssemblyPath(assemblyPath) : null;
    }
}