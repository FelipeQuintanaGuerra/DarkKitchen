using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using IImporters;

namespace BusinessLogic.Reflection;

[ExcludeFromCodeCoverage]
public class AssemblyLoader : IAssemblyLoader
{
    private readonly string _pluginsPath;
    private List<IProductImporter> _importers = new();

    public AssemblyLoader(string pluginsPath)
    {
        _pluginsPath = pluginsPath;
    }

    public List<IProductImporter> LoadImporters()
    {
        _importers = new List<IProductImporter>();

        if (!Directory.Exists(_pluginsPath))
            return _importers;

        var dllFiles = Directory.GetFiles(_pluginsPath, "*.dll");

        foreach (var dllFile in dllFiles)
        {
            Assembly assembly = Assembly.LoadFile(Path.GetFullPath(dllFile));

            var importerTypes = assembly
                .GetTypes()
                .Where(t => t.IsClass && typeof(IProductImporter).IsAssignableFrom(t))
                .ToList();

            foreach (var type in importerTypes)
            {
                var instance = Activator.CreateInstance(type) as IProductImporter;

                if (instance != null)
                    _importers.Add(instance);
            }
        }

        return _importers;
    }

    public List<string> GetImporterNames()
    {
        var importers = LoadImporters();
        return importers.Select(i => i.Name).ToList();
    }

    public IProductImporter? GetImporterByName(string name)
    {
        var importers = LoadImporters();
        return importers.FirstOrDefault(i => i.Name == name);
    }
}
