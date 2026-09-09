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
public class UserServiceTests
{
    private Mock<IGenericRepository<User>> _userRepositoryMock;
    private Mock<ISessionRepository> _sessionRepositoryMock;
    private IUserService _userService;

    [TestInitialize]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IGenericRepository<User>>(MockBehavior.Strict);
        _sessionRepositoryMock = new Mock<ISessionRepository>(MockBehavior.Strict);
        _userService = new UserService(_userRepositoryMock.Object, _sessionRepositoryMock.Object);
    }

    private void SetupUserAdd()
    {
        _userRepositoryMock
            .Setup(r => r.Add(It.IsAny<User>()));
    }

    private void SetupUserUpdate()
    {
        _userRepositoryMock
            .Setup(r => r.Update(It.IsAny<User>()));
    }

    private Session SetupAdminSession(Guid? adminId = null)
    {
        var admin = new User("Admin", "Admin", "admin@mail.com", "099000000", "AdminPassword123!", Role.Admin);
        if (adminId.HasValue)
            admin.Id = adminId.Value;
        var session = new Session(admin);
        _sessionRepositoryMock.Setup(s => s.GetByToken(session.Token)).Returns(session);
        return session;
    }

    #region CreateTests

    [TestMethod]
    public void Create_CreatesUser()
    {
        var session = SetupAdminSession();

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        var dto = new UpdateUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        SetupUserAdd();

        var result = _userService.Create(dto, session.Token);

        Assert.IsNotNull(result);
    }


    [TestMethod]
    public void Create_ExistingUser_Throws()
    {
        var session = SetupAdminSession();

        var existingUser = new User(
            "Juan",
            "Perez",
            "test@mail.com",
            "099123456",
            "ValidPassword123!",
            Role.Client
        );

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(existingUser);

        var dto = new UpdateUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        Assert.ThrowsException<ConflictException>(() =>
            _userService.Create(dto, session.Token));
    }

    #endregion

    #region RegisterTests

    [TestMethod]
    public void Register_WithValidData_CreatesUser()
    {
        var dto = new RegisterUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        User savedUser = null!;

        _userRepositoryMock
            .Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u);

        var result = _userService.Register(dto);

        Assert.IsNotNull(result);
        Assert.IsNotNull(savedUser);
        Assert.AreEqual(dto.Email, result.Email);
        Assert.AreEqual(dto.Name, result.Name);
    }

    [TestMethod]
    public void Register_EmailAlreadyExists_ThrowsException()
    {
        var dto = new RegisterUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        var existingUser = new User(
            "Juan",
            "Perez",
            "test@mail.com",
            "099123456",
            "ValidPassword123!",
            Role.Client
        );

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(existingUser);

        Assert.ThrowsException<ArgumentException>(
            () => _userService.Register(dto)
        );
    }

    [TestMethod]
    public void Register_ValidData_CallsRepositoryAddOnce()
    {
        var dto = new RegisterUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);
        SetupUserAdd();
        _userService.Register(dto);

        _userRepositoryMock.Verify(r => r.Add(It.IsAny<User>()), Times.Once);
    }

    [TestMethod]
    public void Register_StoresHashedPassword_NotPlainText()
    {
        var dto = new RegisterUserDto
        {
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        User savedUser = null!;

        _userRepositoryMock
            .Setup(r => r.Add(It.IsAny<User>()))
            .Callback<User>(u => savedUser = u);

        _userService.Register(dto);

        Assert.IsNotNull(savedUser);
        Assert.AreNotEqual(dto.Password, savedUser.Password);
    }

    #endregion

    #region UpdateTests

    [TestMethod]
    public void Update_Admin_UpdatesUser()
    {
        var session = SetupAdminSession();

        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);
        _userRepositoryMock.Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns(new List<User>());

        SetupUserUpdate();
        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Nuevo",
            Surname = "Apellido",
            Email = "new@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!",
            IsActive = true,
            IsInternal = true
        };


        _userService.Update(dto, session.Token);

        Assert.AreEqual("Nuevo", user.Name);
    }

    [TestMethod]
    public void Update_Self_Throws()
    {
        var id = Guid.NewGuid();
        var session = SetupAdminSession(id);

        var dto = new UpdateUserDto { Id = id };

        Assert.ThrowsException<InvalidOperationException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_UserNotFound_Throws()
    {
        var session = SetupAdminSession();

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!",
            IsActive = true,
            IsInternal = true
        };

        Assert.ThrowsException<KeyNotFoundException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithEmptyName_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "",
            Surname = "Apellido",
            Email = "new@mail.com",
            Phone = "099123456"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithInvalidSurname_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "AB",
            Email = "new@mail.com",
            Phone = "099123456"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithInvalidEmail_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "esto-no-es-email",
            Phone = "099123456"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithInvalidPhone_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "new@mail.com",
            Phone = "123"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithInvalidRole_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "new@mail.com",
            Phone = "099123456",
            Role = (Role)99
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithEmailAlreadyTaken_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);
        var otherUser = new User("Ana", "Gomez", "taken@mail.com", "099654321", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        _userRepositoryMock.Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns(new List<User> { otherUser });

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "taken@mail.com",
            Phone = "099123456"
        };

        Assert.ThrowsException<ConflictException>(() =>
            _userService.Update(dto, session.Token));
    }


    [TestMethod]
    public void Update_Admin_CallsRepositoryUpdate()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);
        _userRepositoryMock.Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns(new List<User>());
        SetupUserUpdate();
        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Nuevo",
            Surname = "Apellido",
            Email = "new@mail.com",
            Phone = "099123456"
        };

        _userService.Update(dto, session.Token);

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }

    [TestMethod]
    public void Update_WithNullSession_Throws()
    {
        _sessionRepositoryMock.Setup(s => s.GetByToken(It.IsAny<Guid>()))
            .Returns((Session?)null);

        var token = Guid.NewGuid();

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "test@mail.com",
            Phone = "099123456"
        };

        Assert.ThrowsException<UnauthorizedAccessException>(() =>
            _userService.Update(dto, token));
    }

    [TestMethod]
    public void Update_WithEmptySurname_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "",
            Email = "new@mail.com",
            Phone = "099123456"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithEmptyEmail_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "",
            Phone = "099123456"
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    [TestMethod]
    public void Update_WithEmptyPhone_Throws()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var dto = new UpdateUserDto
        {
            Id = Guid.NewGuid(),
            Name = "Juan",
            Surname = "Perez",
            Email = "new@mail.com",
            Phone = ""
        };

        Assert.ThrowsException<ArgumentException>(() =>
            _userService.Update(dto, session.Token));
    }

    #endregion

    #region DeleteTests

    [TestMethod]
    public void Delete_Admin_SetsInactive()
    {
        var session = SetupAdminSession();

        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);
        SetupUserUpdate();
        _userService.Delete(user.Id, session.Token);

        Assert.IsFalse(user.IsActive);
    }

    [TestMethod]
    public void Delete_Self_Throws()
    {
        var id = Guid.NewGuid();
        var session = SetupAdminSession(id);

        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Admin);
        user.Id = id;

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        Assert.ThrowsException<InvalidOperationException>(() =>
            _userService.Delete(id, session.Token));
    }

    [TestMethod]
    public void Delete_UserNotFound_Throws()
    {
        var session = SetupAdminSession();

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        var userIdToDelete = Guid.NewGuid();

        Assert.ThrowsException<KeyNotFoundException>(() =>
            _userService.Delete(userIdToDelete, session.Token));
    }

    #endregion

    #region SearchTests

    [TestMethod]
    public void SearchBy_Admin_ReturnsUsers()
    {
        var users = new List<User>
        {
            new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client),
            new User("Ana", "Gomez", "b@mail.com", "099123456", "ValidPassword123!", Role.Client)
        };

        _userRepositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns((Expression<Func<User, bool>> predicate,
                int page,
                int pageSize) =>
            {
                return users
                    .AsQueryable()
                    .Where(predicate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToList();
            });

        var result = _userService.SearchBy("Juan", "Perez");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Juan", result[0].Name);
    }

    [TestMethod]
    public void SearchBy_WhenBothFieldsAreEmpty_ReturnsAllUsers()
    {
        var users = new List<User>
      {
          new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client),
          new User("Ana", "Gomez", "b@mail.com", "099123456", "ValidPassword123!", Role.Client)
      };

        _userRepositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns((Expression<Func<User, bool>> predicate, int page, int pageSize) =>
                users.AsQueryable().Where(predicate).Skip((page - 1) * pageSize).Take(pageSize).ToList());

        var result = _userService.SearchBy("", "");

        Assert.AreEqual(2, result.Count);
    }

    [TestMethod]
    public void SearchBy_WhenPartialNameDifferentCase_ReturnsMatchingUsers()
    {
        var users = new List<User>
      {
          new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client),
          new User("Ana", "Gomez", "b@mail.com", "099123456", "ValidPassword123!", Role.Client)
      };

        _userRepositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns((Expression<Func<User, bool>> predicate, int page, int pageSize) =>
                users.AsQueryable().Where(predicate).Skip((page - 1) * pageSize).Take(pageSize).ToList());

        var result = _userService.SearchBy("ju", "");   // parcial + minúscula                                                                               

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Juan", result[0].Name);
    }

    [TestMethod]
    public void SearchBy_WhenPartialSurname_ReturnsMatchingUsers()
    {
        var users = new List<User>
      {
          new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client),
          new User("Ana", "Gomez", "b@mail.com", "099123456", "ValidPassword123!", Role.Client)
      };

        _userRepositoryMock
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<User, bool>>>(), 1, 20))
            .Returns((Expression<Func<User, bool>> predicate, int page, int pageSize) =>
                users.AsQueryable().Where(predicate).Skip((page - 1) * pageSize).Take(pageSize).ToList());

        var result = _userService.SearchBy("", "gom");

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual("Gomez", result[0].Surname);
    }

    #endregion

    #region GetByIdTests

    [TestMethod]
    public void GetUserById_UserExists_ReturnsUserSummaryDto()
    {
        var id = Guid.NewGuid();

        var user = new User(
            "Juan",
            "Perez",
            "test@mail.com",
            "099123456",
            "ValidPassword123!",
            Role.Client
        );

        user.Id = id;

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);

        var result = _userService.GetUserById(id);

        Assert.IsNotNull(result);
        Assert.AreEqual(user.Id, result.Id);
        Assert.AreEqual(user.Name, result.Name);
        Assert.AreEqual(user.Surname, result.Surname);
        Assert.AreEqual(user.Email, result.Email);
        Assert.AreEqual(user.Role, result.Role);
    }

    [TestMethod]
    public void GetUserById_UserNotFound_Throws()
    {
        var id = Guid.NewGuid();

        _userRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns((User?)null);

        Assert.ThrowsException<KeyNotFoundException>(() =>
            _userService.GetUserById(id));
    }

    [TestMethod]
    public void Delete_Admin_CallsRepositoryUpdate()
    {
        var session = SetupAdminSession();
        var user = new User("Juan", "Perez", "a@mail.com", "099123456", "ValidPassword123!", Role.Client);

        _userRepositoryMock.Setup(r => r.GetBy(It.IsAny<Expression<Func<User, bool>>>()))
            .Returns(user);
        SetupUserUpdate();
        _userService.Delete(user.Id, session.Token);

        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }

    #endregion
}