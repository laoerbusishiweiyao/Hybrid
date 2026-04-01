using System.Reflection;
using System.Runtime.Loader;
using Serilog;

namespace Chaos;

public sealed class CodeLoader : Singleton<CodeLoader>, ISingletonAwake
{
    private AssemblyLoadContext assemblyLoadContext;

    private readonly List<Assembly> assemblies = new();

    public void Awake()
    {
    }

    public void Start()
    {
        HashSet<string> assemblyNames =
        [
            "Foundation",
            "Loader",
            "Model"
        ];

        var domainAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        foreach (var assembly in domainAssemblies)
        {
            var name = assembly.GetName().Name;
            if (assemblyNames.Contains(name))
            {
                assemblies.Add(assembly);
            }
        }

        var hotfixAssembly = LoadHotfix();

        List<Assembly> list =
        [
            ..assemblies,
            hotfixAssembly
        ];
        World.Default.AddSingleton<CodeTypeRegistry, Assembly[]>(list.ToArray());

        var invoker = new StaticMethodInvoker(hotfixAssembly, "Chaos.Entry", "Start");
        invoker.Run();
    }

    private Assembly LoadHotfix()
    {
        assemblyLoadContext?.Unload();
        GC.Collect();
        assemblyLoadContext = new AssemblyLoadContext("Hotfix", true);
        var dllBytes = File.ReadAllBytes("Hotfix.dll");
        var pdbBytes = File.ReadAllBytes("Hotfix.pdb");
        var hotfixAssembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
        return hotfixAssembly;
    }

    public void Reload()
    {
        var hotfixAssembly = LoadHotfix();

        List<Assembly> list =
        [
            ..assemblies,
            hotfixAssembly
        ];
        var codeTypeRegistry = World.Default.AddSingleton<CodeTypeRegistry, Assembly[]>(list.ToArray());
        codeTypeRegistry.Execute();
        Log.Debug("Reload dll finish.");
    }
}