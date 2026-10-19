using System.Linq.Expressions;
using BusinessLogic;
using Domain;
using Domain.Enums;
using DTOs.User;
using IBussinesLogic;
using IRepository;
using Moq;

namespace BussinesLogicTests;

[TestClass]
public class SessionServiceTests
{
    private Mock<IGenericRepository<User>> _userRepositoryMock = null!;
    private Mock<ISessionRepository> _sessionRepositoryMock = null!;
    private ISessionService _sessionService = null!;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IGenericRepository<User>>(MockBehavior.Strict);
        _sessionRepositoryMock = new Mock<ISessionRepository>(MockBehavior.Strict);
        _sessionService = new SessionService(_userRepositoryMock.Object, _sessionRepositoryMock.Object);
    }

    #region LoginTests

    [TestMethod]
    public void Login_WithValidCredentials_ReturnsLoginResponse()
    {
        var dto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "ValidPassword123!"
        };

        var user = new User("Juan", "Perez", "test@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> predicate) =>
                predicate.Compile()(user) ? user : null
            );

        _sessionRepositoryMock
            .Setup(r => r.Add(It.IsAny<Session>()));

        var result = _sessionService.Login(dto);

        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(LoginResponseDto));
    }

    [TestMethod]
    public void Login_NullDto_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(
            () => _sessionService.Login(null!)
        );
    }

    [TestMethod]
    public void Login_EmptyEmail_ThrowsArgumentException()
    {
        var dto = new LoginDto
        {
            Email = "",
            Password = "ValidPassword123!"
        };

        Assert.ThrowsException<ArgumentException>(
            () => _sessionService.Login(dto)
        );
    }

    [TestMethod]
    public void Login_EmptyPassword_ThrowsArgumentException()
    {
        var dto = new LoginDto
        {
            Email = "test@mail.com",
            Password = ""
        };

        Assert.ThrowsException<ArgumentException>(
            () => _sessionService.Login(dto)
        );
    }

    [TestMethod]
    public void Login_UserDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto
        {
            Email = "wrong@mail.com",
            Password = "ValidPassword123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _sessionService.Login(dto)
        );
    }

    [TestMethod]
    public void Login_WrongPassword_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "WrongPassword123!"
        };

        var user = new User("Juan", "Perez", "test@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _sessionService.Login(dto)
        );
    }

    [TestMethod]
    public void Login_UserInactive_ThrowsUnauthorizedAccessException()
    {
        var dto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "ValidPassword123!"
        };

        var user = new User("Juan", "Perez", "test@mail.com", "099123456", "ValidPassword123!", Role.Client);
        user.IsActive = false;

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _sessionService.Login(dto)
        );
    }

    [TestMethod]
    public void Login_WithValidCredentials_CreatesSessionAndReturnsToken()
    {
        var dto = new LoginDto
        {
            Email = "test@mail.com",
            Password = "ValidPassword123!"
        };

        var user = new User("Juan", "Perez", "test@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((Expression<Func<User, bool>> predicate) =>
                predicate.Compile()(user) ? user : null
            );

        _sessionRepositoryMock
            .Setup(r => r.Add(It.IsAny<Session>()));

        var result = _sessionService.Login(dto);

        Assert.IsNotNull(result.Token);
        Assert.AreNotEqual(Guid.Empty, result.Token);
        Assert.IsNotNull(result.User);
        Assert.AreEqual(user.Email, result.User.Email);

        _sessionRepositoryMock.Verify(
            r => r.Add(It.Is<Session>(s => s.User.Email == user.Email)),
            Times.Once
        );
    }

    #endregion

    #region LogoutTests

    [TestMethod]
    public void Logout_RemovesSessionByToken()
    {
        var token = Guid.NewGuid();

        _sessionRepositoryMock
            .Setup(r => r.Remove(token));

        _sessionService.Logout(token);

        _sessionRepositoryMock.Verify(
            r => r.Remove(token),
            Times.Once
        );
    }

    #endregion
}
