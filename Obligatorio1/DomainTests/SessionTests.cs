namespace DomainTests;

using Domain;
using Domain.Enums;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class SessionTests
{
    private User _validUser;

    [TestInitialize]
    public void Setup()
    {
        _validUser = new User("Juan", "Garcia", "juan@mail.com", "099123456", "MiPassword@Ab789", Role.Client);
    }

    [TestMethod]
    public void CreateSession_WithValidUser_ShouldGenerateToken()
    {
        var session = new Session(_validUser);

        Assert.IsNotNull(session.Token);
        Assert.AreNotEqual(Guid.Empty, session.Token);
        Assert.AreEqual(_validUser.Id, session.UserId);
        Assert.AreEqual(_validUser, session.User);
    }

    [TestMethod]
    public void CreateSession_WithNullUser_ShouldThrowException()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Session(null)
        );
    }
}