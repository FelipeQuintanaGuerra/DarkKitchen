using IImporters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XmlImporter;

namespace XmlImporterTests;

[TestClass]
public class XmlProductImporterTests
{
    [TestMethod]
    public void Name_ReturnsXmlImporter()
    {
        var importer = new XmlProductImporter();

        Assert.AreEqual("XML Importer", importer.Name);
    }

    [TestMethod]
    public void GetProducts_ReturnsProductsFromXmlFile()
    {
        var xml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Products>
    <Product>
        <Code>P001</Code>
        <Name>Pizza Margarita Especial</Name>
        <Description>Pizza clasica con tomate y mozzarella fresca</Description>
        <Price>350.00</Price>
        <CommercialLineName>Pizzas</CommercialLineName>
        <CategoryName>Comida</CategoryName>
        <ImagePaths>
            <Path>pizza1.jpg</Path>
            <Path>pizza2.jpg</Path>
        </ImagePaths>
    </Product>
    <Product>
        <Code>P002</Code>
        <Name>Hamburguesa Doble Queso</Name>
        <Description>Doble carne con queso cheddar fundido</Description>
        <Price>420.50</Price>
        <CommercialLineName>Burgers</CommercialLineName>
        <CategoryName>Comida</CategoryName>
        <ImagePaths>
            <Path>burger1.jpg</Path>
        </ImagePaths>
    </Product>
</Products>";

        var tempFile = Path.GetTempFileName();
        File.WriteAllText(tempFile, xml);

        try
        {
            var importer = new XmlProductImporter();
            var products = importer.GetProducts(tempFile);

            Assert.AreEqual(2, products.Count);

            Assert.AreEqual("P001", products[0].Code);
            Assert.AreEqual("Pizza Margarita Especial", products[0].Name);
            Assert.AreEqual("Pizza clasica con tomate y mozzarella fresca", products[0].Description);
            Assert.AreEqual(350.00m, products[0].Price);
            Assert.AreEqual("Pizzas", products[0].CommercialLineName);
            Assert.AreEqual("Comida", products[0].CategoryName);
            Assert.AreEqual(2, products[0].ImagePaths.Count);
            Assert.AreEqual("pizza1.jpg", products[0].ImagePaths[0]);

            Assert.AreEqual("P002", products[1].Code);
            Assert.AreEqual("Hamburguesa Doble Queso", products[1].Name);
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
        var importer = new XmlProductImporter();

        importer.GetProducts("nonexistent_file.xml");
    }
}
