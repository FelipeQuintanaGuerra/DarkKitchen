using System.Text.Json;
using IImporters;

namespace JsonImporter;

public class JsonProductImporter : IProductImporter
{
    public string Name => "JSON Importer";

    public List<ImportedProduct> GetProducts(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        var json = File.ReadAllText(path);

        var products = JsonSerializer.Deserialize<List<ImportedProduct>>(json)
            ?? new List<ImportedProduct>();

        return products;
    }
}
