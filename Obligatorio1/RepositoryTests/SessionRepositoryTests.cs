using Domain;
using Domain.Enums;
using IRepository;
using Repository;

namespace RepositoriesTests;

[TestClass]
public class SessionRepositoryTests : RepositoryTestBase
{
    private ISessionRepository _sessionRepository = null!;

    [TestInitialize]
    public void InitializeRepository()
    {
        _sessionRepository = new SessionRepository(Context);
    }

    [TestMethod]
    public void Add_SavesSessionToDatabase()
    {
        var user = CreateUser();
        Context.Users.Add(user);
        Context.SaveChanges();

        var session = new Session(user);
        _sessionRepository.Add(session);

        var savedSession = Context.Sessions.FirstOrDefault(s => s.Token == session.Token);

        Assert.IsNotNull(savedSession);
        Assert.AreEqual(user.Id, savedSession.UserId);
    }

    [TestMethod]
    public void GetByToken_ReturnsSessionWithUser()
    {
        var user = CreateUser();
        Context.Users.Add(user);
        Context.SaveChanges();

        var session = new Session(user);
        _sessionRepository.Add(session);

        var result = _sessionRepository.GetByToken(session.Token);

        Assert.IsNotNull(result);
        Assert.AreEqual(session.Token, result.Token);
        Assert.IsNotNull(result.User);
        Assert.AreEqual(user.Email, result.User.Email);
    }

    [TestMethod]
    public void GetByToken_ReturnsNullWhenNotFound()
    {
        var result = _sessionRepository.GetByToken(Guid.NewGuid());

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Remove_DeletesSessionFromDatabase()
    {
        var user = CreateUser();
        Context.Users.Add(user);
        Context.SaveChanges();

        var session = new Session(user);
        _sessionRepository.Add(session);

        _sessionRepository.Remove(session.Token);

        var result = Context.Sessions.FirstOrDefault(s => s.Token == session.Token);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Remove_DoesNothingWhenTokenNotFound()
    {
        _sessionRepository.Remove(Guid.NewGuid());
    }

    private static User CreateUser()
    {
        return new User("Juan", "Perez", "test@mail.com", "099123456", "ValidPassword!Ab7", Role.Client);
    }
}