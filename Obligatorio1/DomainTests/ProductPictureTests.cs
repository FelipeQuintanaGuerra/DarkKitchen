using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class ProductPictureTests
{
    private string _fileName = "foto.jpg";
    private int _size = 400;

    [TestMethod]
    public void NewProductPictureWithInvalidExtensionReturnsError()
    {
        // Arrange
        string invalidFile = "foto.png";

        // Assert
        Assert.ThrowsException<ArgumentException>(() => new ProductPicture(invalidFile, _size));
    }
    [TestMethod]
    public void NewProductPictureWithEmptyNameReturnsError()
    {
        // Arrange
        string emptyName = "";

        // Assert
        Assert.ThrowsException<ArgumentNullException>(() => new ProductPicture(emptyName, _size));
    }
    [TestMethod]
    public void NewProductPictureWithSizeGreaterThan500KbReturnsError()
    {
        // Arrange
        int invalidSize = 501;

        // Assert
        Assert.ThrowsException<ArgumentException>(() => new ProductPicture(_fileName, invalidSize));
    }
}