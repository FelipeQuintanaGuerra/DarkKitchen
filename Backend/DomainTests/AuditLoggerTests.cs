using Domain;
using Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class AuditLoggerTests
{
    [TestMethod]
    public void NewAuditLoggerWithNullNameThrowsArgumentNullException()
    {
        Assert.ThrowsException<ArgumentException>(() => new AuditLogger("", "string", "PRODUCTO", new Guid(), "Se creó el producto X"));
    }

    [TestMethod]
    public void NewAuditLoggerWithEmptyActionThrowsArgumentException()
    {
        User validUser = new User("Juan", "Garcia", "juan@mail.com", "099123456", "MiPassword@Ab789", Role.Client);

        Assert.ThrowsException<ArgumentException>(() => new AuditLogger("Juan", "", "PRODUCTO", new Guid(), "Se creó el producto X"));
    }

    [TestMethod]
    public void NewAuditLogger_WithAllFields_ShouldStoreValuesCorrectly()
    {
        var entityId = Guid.NewGuid();
        var audit = new AuditLogger("admin", "Alta", "PRODUCTO", entityId, "Se creó el producto X");

        Assert.AreEqual("admin", audit.Username);
        Assert.AreEqual("Alta", audit.Action);
        Assert.AreEqual("PRODUCTO", audit.EntityName);
        Assert.AreEqual(entityId, audit.EntityId);
        Assert.AreEqual("Se creó el producto X", audit.Description);
        Assert.AreNotEqual(Guid.Empty, audit.Id);
        Assert.AreNotEqual(default(DateTime), audit.Time);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewAuditLogger_WithEmptyEntityName_ShouldThrowException()
    {
        new AuditLogger("admin", "Alta", "", Guid.NewGuid(), "descripcion");
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewAuditLogger_WithEmptyDescription_ShouldThrowException()
    {
        new AuditLogger("admin", "Alta", "PRODUCTO", Guid.NewGuid(), "");
    }
}