namespace IBussinesLogic;

public interface IImporterService
{
    List<string> GetImporterNames();
    int ImportProducts(string importerName, string filePath);
}
