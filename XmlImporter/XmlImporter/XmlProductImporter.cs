using System.Globalization;
using System.Xml.Linq;
using IImporters;

namespace XmlImporter;

public class XmlProductImporter : IProductImporter
{
    public string Name => "XML Importer";

    public List<ImportedProduct> GetProducts(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"File not found: {path}");

        var doc = XDocument.Load(path);
        var products = new List<ImportedProduct>();

        foreach (var element in doc.Descendants("Product"))
        {
            var product = new ImportedProduct
            {
                Code = element.Element("Code")?.Value ?? string.Empty,
                Name = element.Element("Name")?.Value ?? string.Empty,
                Description = element.Element("Description")?.Value ?? string.Empty,
                Price = decimal.Parse(element.Element("Price")?.Value ?? "0", CultureInfo.InvariantCulture),
                CommercialLineName = element.Element("CommercialLineName")?.Value ?? string.Empty,
                CategoryName = element.Element("CategoryName")?.Value ?? string.Empty,
                ImagePaths = element.Element("ImagePaths")?
                    .Elements("Path")
                    .Select(p => p.Value)
                    .ToList() ?? new List<string>()
            };

            products.Add(product);
        }

        return products;
    }
}
