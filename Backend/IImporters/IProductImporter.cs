namespace IImporters;

public interface IProductImporter
{
    string Name { get; }
    List<ImportedProduct> GetProducts(string path);
}
