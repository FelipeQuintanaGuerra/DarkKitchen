using System.Linq.Expressions;
using BusinessLogic;
using Domain;
using DTOs.AuditDTOs;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using IRepository;

namespace BusinessLogicTests;

[TestClass]
public class AuditLoggerServiceTests
{
    private Mock<IGenericRepository<AuditLogger>> _repositoryMock = null!;
    private AuditLoggerService _service = null!;

    [TestInitialize]
    public void Initialize()
    {
        _repositoryMock = new Mock<IGenericRepository<AuditLogger>>(MockBehavior.Strict);
        _service = new AuditLoggerService(_repositoryMock.Object);
    }

    [TestMethod]
    public async Task LogAsync_ShouldAddLogToRepository()
    {
        var entityId = Guid.NewGuid();

        var dto = new CreateLogDto
        {
            Username = "felipe",
            Action = "Alta",
            EntityName = "PRODUCTO",
            EntityId = entityId,
            EntityDisplayName = "Pizza"
        };

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<AuditLogger>()));

        await _service.LogAsync(dto);

        _repositoryMock.Verify(
            r => r.Add(It.Is<AuditLogger>(
                a => a.Username == dto.Username &&
                     a.Action == dto.Action &&
                     a.EntityName == dto.EntityName &&
                     a.EntityId == dto.EntityId)),
            Times.Once);
    }

    [TestMethod]
    public async Task LogAsync_ShouldGenerateAndSaveDescription()
    {
        var entityId = Guid.NewGuid();

        var dto = new CreateLogDto
        {
            Username = "admin",
            Action = "Alta",
            EntityName = "PRODUCTO",
            EntityId = entityId,
            EntityDisplayName = "Pizza"
        };

        AuditLogger? savedEntity = null;

        _repositoryMock
            .Setup(r => r.Add(It.IsAny<AuditLogger>()))
            .Callback<AuditLogger>(a => savedEntity = a);

        await _service.LogAsync(dto);

        Assert.IsNotNull(savedEntity);
        Assert.AreEqual("admin", savedEntity.Username);
        Assert.AreEqual("Alta", savedEntity.Action);
        Assert.AreEqual("PRODUCTO", savedEntity.EntityName);
        Assert.AreEqual(entityId, savedEntity.EntityId);

        StringAssert.Contains(savedEntity.Description, "Se creó producto");
        StringAssert.Contains(savedEntity.Description, "Pizza");
        StringAssert.Contains(savedEntity.Description, entityId.ToString());
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Search_WhenFromIsGreaterThanTo_ShouldThrowException()
    {
        var dto = new SearchAuditRequestDto
        {
            From = new DateTime(2026, 6, 1),
            To = new DateTime(2026, 5, 1),
            EntityName = "PRODUCTO",
            EntityId = Guid.NewGuid()
        };

        _service.Search(dto);
    }

    [TestMethod]
    public void Search_WithValidFilters_ShouldReturnFilteredResults()
    {
        var entityId = Guid.NewGuid();
        var audit = new AuditLogger("admin", "Alta", "PRODUCTO", entityId, "Se creó producto");

        _repositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<System.Linq.Expressions.Expression<Func<AuditLogger, bool>>>(), 1, 20))
            .Returns(new List<AuditLogger> { audit });

        var dto = new SearchAuditRequestDto
        {
            From = DateTime.Now.AddHours(-1),
            To = DateTime.Now.AddHours(1),
            EntityName = "PRODUCTO",
            EntityId = entityId
        };

        var result = _service.Search(dto);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("PRODUCTO", result[0].EntityName);
        Assert.AreEqual(entityId, result[0].EntityId);
        Assert.AreEqual("Se creó producto", result[0].Description);
        Assert.AreEqual("admin", result[0].Username);
    }

    [TestMethod]
    public void Search_WhenEntityIdIsEmpty_IgnoresEntityIdFilter()
    {
        var log = new AuditLogger("admin", "Alta", "PRODUCTO", Guid.NewGuid(), "Se creó producto");
        var logs = new List<AuditLogger> { log };

        _repositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<AuditLogger, bool>>>(), 1, 20))
            .Returns((Expression<Func<AuditLogger, bool>> predicate, int page, int pageSize) =>
                logs.AsQueryable().Where(predicate).Skip((page - 1) * pageSize).Take(pageSize).ToList());

        var dto = new SearchAuditRequestDto
        {
            From = DateTime.Now.AddHours(-1),
            To = DateTime.Now.AddHours(1),
            EntityName = "PRODUCTO",
            EntityId = Guid.Empty
        };

        var result = _service.Search(dto);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void Search_WhenEntityNameIsEmpty_IgnoresEntityNameFilter()
    {
        var log = new AuditLogger("admin", "Alta", "PROMOCION", Guid.NewGuid(), "desc");
        var logs = new List<AuditLogger> { log };

        _repositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<AuditLogger, bool>>>(), 1, 20))
            .Returns((Expression<Func<AuditLogger, bool>> predicate, int page, int pageSize) =>
                logs.AsQueryable().Where(predicate).Skip((page - 1) * pageSize).Take(pageSize).ToList());

        var dto = new SearchAuditRequestDto
        {
            From = DateTime.Now.AddHours(-1),
            To = DateTime.Now.AddHours(1),
            EntityName = "",
            EntityId = Guid.Empty
        };

        var result = _service.Search(dto);

        Assert.AreEqual(1, result.Count);
    }
}