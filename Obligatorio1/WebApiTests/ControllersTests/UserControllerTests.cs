using Domain.Enums;
using DTOs.User;
using IBussinesLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests.ControllersTests;

[TestClass]
public class UserControllerTests
{
    private Mock<IUserService> _userServiceMock;
    private UserController _controller;

    [TestInitialize]
    public void Setup()
    {
        _userServiceMock = new Mock<IUserService>();

        _controller = new UserController(
            _userServiceMock.Object
        );

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    #region GetUserByIdTests

    [TestMethod]
    public void GetUserById_UserExists_ReturnsOk()
    {
        var id = Guid.NewGuid();

        var userDto = new UserSummaryDto
        {
            Id = id,
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Role = Role.Client
        };

        _userServiceMock
            .Setup(s => s.GetUserById(id))
            .Returns(userDto);

        var result = _controller.GetUserById(id);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(userDto, okResult.Value);
    }

    [TestMethod]
    public void GetUserById_CallsUserService()
    {
        var id = Guid.NewGuid();

        _userServiceMock
            .Setup(s => s.GetUserById(id))
            .Returns(new UserSummaryDto { Id = id });

        _controller.GetUserById(id);

        _userServiceMock.Verify(s => s.GetUserById(id), Times.Once);
    }

    #endregion

    #region DeleteTests

    [TestMethod]
    public void DeleteUser_ValidId_ReturnsNoContent()
    {
        var id = Guid.NewGuid();

        _userServiceMock.Setup(s => s.Delete(id, It.IsAny<Guid>()));

        var result = _controller.DeleteUser(id);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    [TestMethod]
    public void DeleteUser_CallsUserService()
    {
        var id = Guid.NewGuid();

        _userServiceMock.Setup(s => s.Delete(id, It.IsAny<Guid>()));

        _controller.DeleteUser(id);

        _userServiceMock.Verify(s => s.Delete(id, It.IsAny<Guid>()), Times.Once);
    }

    #endregion

    #region UpdateTests

    [TestMethod]
    public void UpdateUser_ValidDto_ReturnsOk()
    {
        var id = Guid.NewGuid();
        var dto = CreateValidDto();

        _userServiceMock.Setup(s => s.Update(dto, It.IsAny<Guid>()));

        var result = _controller.UpdateUser(id, dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public void UpdateUser_SetsIdFromRoute()
    {
        var id = Guid.NewGuid();
        var dto = CreateValidDto();

        _userServiceMock.Setup(s => s.Update(It.IsAny<UpdateUserDto>(), It.IsAny<Guid>()));

        _controller.UpdateUser(id, dto);

        Assert.AreEqual(id, dto.Id);
    }

    [TestMethod]
    public void UpdateUser_CallsUserService()
    {
        var id = Guid.NewGuid();
        var dto = CreateValidDto();

        _userServiceMock.Setup(s => s.Update(dto, It.IsAny<Guid>()));

        _controller.UpdateUser(id, dto);

        _userServiceMock.Verify(s => s.Update(dto, It.IsAny<Guid>()), Times.Once);
    }

    #endregion

    #region CreateTests

    [TestMethod]
    public void Create_ValidDto_ReturnsCreated()
    {
        var dto = new UpdateUserDto()
        {
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        var expectedUser = new UserSummaryDto { Email = dto.Email };

        _userServiceMock
            .Setup(s => s.Create(dto, It.IsAny<Guid>()))
            .Returns(expectedUser);

        var result = _controller.Create(dto);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);
        Assert.AreEqual(expectedUser, createdResult.Value);
    }

    [TestMethod]
    public void Create_CallsUserService()
    {
        var dto = new UpdateUserDto()
        {
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "ValidPassword123!"
        };

        var expectedUser = new UserSummaryDto { Email = dto.Email };

        _userServiceMock
            .Setup(s => s.Create(dto, It.IsAny<Guid>()))
            .Returns(expectedUser);

        _controller.Create(dto);

        _userServiceMock.Verify(s => s.Create(dto, It.IsAny<Guid>()), Times.Once);
    }

    #endregion

    #region RegisterTests

    [TestMethod]
    public void Register_ValidDto_ReturnsCreated()
    {
        var dto = new RegisterUserDto
        {
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Phone = "123456",
            Password = "1234"
        };

        var expectedUser = new UserSummaryDto
        {
            Name = dto.Name,
            Surname = dto.Surname,
            Email = dto.Email
        };

        _userServiceMock
            .Setup(s => s.Register(dto))
            .Returns(expectedUser);

        var result = _controller.Register(dto);

        var createdResult = result as ObjectResult;

        Assert.IsNotNull(createdResult);
        Assert.AreEqual(201, createdResult.StatusCode);

        var returnedUser = createdResult.Value as UserSummaryDto;
        Assert.IsNotNull(returnedUser);
        Assert.AreEqual(expectedUser.Email, returnedUser.Email);
        Assert.AreEqual(expectedUser.Name, returnedUser.Name);
        Assert.AreEqual(expectedUser.Surname, returnedUser.Surname);
    }

    [TestMethod]
    public void Register_CallsSessionService()
    {
        var dto = new RegisterUserDto
        {
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Phone = "123456",
            Password = "1234"
        };

        var expectedUser = new UserSummaryDto { Email = dto.Email };

        _userServiceMock
            .Setup(s => s.Register(dto))
            .Returns(expectedUser);

        _controller.Register(dto);

        _userServiceMock.Verify(s => s.Register(dto), Times.Once);
    }

    #endregion

    #region SearchByTests

    [TestMethod]
    public void SearchBy_ReturnsOk()
    {
        var users = new List<UserSummaryDto>
        {
            new UserSummaryDto { Name = "Felipe", Surname = "Quintana" }
        };

        _userServiceMock
            .Setup(s => s.SearchBy("Felipe", "Quintana"))
            .Returns(users);

        var result = _controller.SearchBy("Felipe", "Quintana");

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);
    }

    [TestMethod]
    public void SearchBy_CallsUserService()
    {
        _userServiceMock
            .Setup(s => s.SearchBy("Felipe", "Quintana"))
            .Returns(new List<UserSummaryDto>());

        _controller.SearchBy("Felipe", "Quintana");

        _userServiceMock.Verify(s => s.SearchBy("Felipe", "Quintana"), Times.Once);
    }


    #endregion

    private UpdateUserDto CreateValidDto()
    {
        return new UpdateUserDto
        {
            Name = "Felipe",
            Surname = "Quintana",
            Email = "test@mail.com",
            Phone = "099123456",
            Password = "Felipe1234.",
            IsActive = true,
            IsInternal = false
        };
    }
}