using IImporters;

namespace BusinessLogic.Reflection;

public interface IAssemblyLoader
{
    List<string> GetImporterNames();
    IProductImporter? GetImporterByName(string name);
}
