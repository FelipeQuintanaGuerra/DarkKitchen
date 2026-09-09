namespace IImporters;

public class ImportedProduct
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string CommercialLineName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public List<string> ImagePaths { get; set; } = new();
}
