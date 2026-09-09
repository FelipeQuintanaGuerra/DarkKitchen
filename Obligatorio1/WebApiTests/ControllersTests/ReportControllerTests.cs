using DTOs.ReportDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests.ControllersTests;

[TestClass]
public class ReportControllerTests
{
    private Mock<IReportService> _reportServiceMock;
    private ReportController _controller;

    [TestInitialize]
    public void Setup()
    {
        _reportServiceMock = new Mock<IReportService>();
        _controller = new ReportController(_reportServiceMock.Object);
    }

    #region TopProductsTests

    [TestMethod]
    public void GetTopProducts_WithResults_ReturnsOk()
    {
        var from = DateTime.Now.AddDays(-10);
        var to = DateTime.Now;

        var products = new List<TopProductDto>
        {
            new TopProductDto
            {
                Code = "P1",
                Name = "Product 1",
                Quantity = 10,
                Images = new List<string> { "Img1.png" }
            }
        };

        _reportServiceMock
            .Setup(s => s.GetTopProducts(from, to))
            .Returns(products);

        var result = _controller.GetTopProducts(from, to);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(products, okResult.Value);
    }

    [TestMethod]
    public void GetTopProducts_EmptyList_ReturnsOk()
    {
        var from = DateTime.Now.AddDays(-10);
        var to = DateTime.Now;

        _reportServiceMock
            .Setup(s => s.GetTopProducts(from, to))
            .Returns(new List<TopProductDto>());

        var result = _controller.GetTopProducts(from, to);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
    }

    #endregion

    #region SalesReporTests

    [TestMethod]
    public void GetSalesReport_WithResults_ReturnsOk()
    {
        var from = DateTime.Now.AddDays(-10);
        var to = DateTime.Now;

        var report = new List<MonthlySalesDto> { new MonthlySalesDto() };

        _reportServiceMock
            .Setup(s => s.GetSalesReport(from, to))
            .Returns(report);

        var result = _controller.GetSalesReport(from, to);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(report, okResult.Value);
    }

    [TestMethod]
    public void GetSalesReport_EmptyList_ReturnsOk()
    {
        var from = DateTime.Now.AddDays(-10);
        var to = DateTime.Now;
        var report = new List<MonthlySalesDto> { new MonthlySalesDto() };

        _reportServiceMock
            .Setup(s => s.GetSalesReport(from, to))
            .Returns(report);

        var result = _controller.GetSalesReport(from, to);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
    }
    #endregion

}