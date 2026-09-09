using Domain;
using DTOs.AuditDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests.ControllersTests;

[TestClass]
public class AuditControllerTests
{
    private Mock<IAuditLoggerService> _auditServiceMock = null!;
    private AuditController _controller = null!;

    [TestInitialize]
    public void Setup()
    {
        _auditServiceMock = new Mock<IAuditLoggerService>();
        _controller = new AuditController(_auditServiceMock.Object);
    }

    [TestMethod]
    public void Search_WithValidFilters_ReturnsOk()
    {
        var entityId = Guid.NewGuid();
        var dto = new SearchAuditRequestDto
        {
            From = DateTime.Now.AddHours(-1),
            To = DateTime.Now.AddHours(1),
            EntityName = "PRODUCTO",
            EntityId = entityId
        };

        var expectedList = new List<AuditLogResponseDto>
        {
            new AuditLogResponseDto
            {
                Id = Guid.NewGuid(),
                Time = DateTime.Now,
                EntityName = "PRODUCTO",
                EntityId = entityId,
                Description = "Se creó producto",
                Username = "admin"
            }
        };

        _auditServiceMock
            .Setup(s => s.Search(dto))
            .Returns(expectedList);

        var result = _controller.Search(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(expectedList, okResult.Value);
    }

    [TestMethod]
    public void Search_CallsService()
    {
        var dto = new SearchAuditRequestDto
        {
            From = DateTime.Now.AddHours(-1),
            To = DateTime.Now.AddHours(1),
            EntityName = "PRODUCTO",
            EntityId = Guid.NewGuid()
        };

        _auditServiceMock
            .Setup(s => s.Search(dto))
            .Returns(new List<AuditLogResponseDto>());

        _controller.Search(dto);

        _auditServiceMock.Verify(s => s.Search(dto), Times.Once);
    }
}
