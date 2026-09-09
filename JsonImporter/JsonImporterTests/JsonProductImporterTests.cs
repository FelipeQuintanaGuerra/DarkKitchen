using IImporters;
using JsonImporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JsonImporterTests;

[TestClass]
public class JsonProductImporterTests
{
    [TestMethod]
    public void Name_ReturnsJsonImporter()
    {
        var importer = new JsonProductImporter();

        Assert.AreEqual("JSON Importer", importer.Name);
    }

    [TestMethod]
    public void GetProducts_ReturnsProductsFromJsonFile()
    {
        var json = @"[
            {
                ""Code"": ""P001"",
                ""Name"": ""Pizza Margarita"",
                ""Description"": ""Pizza clasica con tomate y mozzarella"",
                ""Price"": 350.00,
                ""CommercialLineName"": ""Pizzas"",
                ""CategoryName"": ""Comida"",
                ""ImagePaths"": [""pizza1.jpg"", ""pizza2.jpg""]
            },
            {
                ""Code"": ""P002"",
                ""Name"": ""Hamburguesa Doble"",
                ""Description"": ""Doble carne con queso"",
                ""Price"": 420.50,
                ""CommercialLineName"": ""Burgers"",
                ""CategoryName"": ""Comida"",
                ""ImagePaths"": [""burger1.jpg""]
            }
        ]";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, json);

        try
        {
            var importer = new JsonProductImporter();
            var products = importer.GetProducts(tempFile);

            Assert.AreEqual(2, products.Count);

            Assert.AreEqual("P001", products[0].Code);
            Assert.AreEqual("Pizza Margarita", products[0].Name);
            Assert.AreEqual("Pizza clasica con tomate y mozzarella", products[0].Description);
            Assert.AreEqual(350.00m, products[0].Price);
            Assert.AreEqual("Pizzas", products[0].CommercialLineName);
            Assert.AreEqual("Comida", products[0].CategoryName);
            Assert.AreEqual(2, products[0].ImagePaths.Count);
            Assert.AreEqual("pizza1.jpg", products[0].ImagePaths[0]);

            Assert.AreEqual("P002", products[1].Code);
            Assert.AreEqual("Hamburguesa Doble", products[1].Name);
            Assert.AreEqual(420.50m, products[1].Price);
            Assert.AreEqual(1, products[1].ImagePaths.Count);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [TestMethod]
    [ExpectedException(typeof(FileNotFoundException))]
    public void GetProducts_ThrowsWhenFileNotFound()
    {
        var importer = new JsonProductImporter();

        importer.GetProducts("nonexistent_file.json");
    }
}
