using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests.ControllersTests;

[TestClass]
public class ImporterControllerTests
{
    private Mock<IImporterService> _importerServiceMock;
    private ImporterController _controller;

    [TestInitialize]
    public void Setup()
    {
        _importerServiceMock = new Mock<IImporterService>();
        _controller = new ImporterController(_importerServiceMock.Object);
    }

    [TestMethod]
    public void GetImporterNames_ReturnsOkWithNames()
    {
        var names = new List<string> { "JSON Importer", "XML Importer" };
        _importerServiceMock.Setup(s => s.GetImporterNames()).Returns(names);

        var result = _controller.GetImporterNames();

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        CollectionAssert.AreEqual(names, (List<string>)okResult.Value!);
    }

    [TestMethod]
    public void ImportProducts_ReturnsOkWithImportedCount()
    {
        _importerServiceMock
            .Setup(s => s.ImportProducts("JSON Importer", "products.json"))
            .Returns(1);

        var result = _controller.ImportProducts("JSON Importer", "products.json");

        var okResult = result as OkObjectResult;
        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }
}
