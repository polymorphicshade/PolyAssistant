using System.Reflection;

namespace PolyAssistant.Core.Services.Interfaces;

public interface IAssemblyLoadingService
{
    Assembly Load(string assemblyPath);
}