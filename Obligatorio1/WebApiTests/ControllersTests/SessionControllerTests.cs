using DTOs.User;
using IBussinesLogic;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using WebApi.Controllers;

namespace WebApiTests.ControllersTests;

[TestClass]
public class SessionControllerTests
{
    private Mock<ISessionService> _sessionServiceMock;
    private SessionController _controller;

    [TestInitialize]
    public void Setup()
    {
        _sessionServiceMock = new Mock<ISessionService>();

        _controller = new SessionController(
            _sessionServiceMock.Object
        );
    }

    #region LoginTests

    [TestMethod]
    public void Login_ValidCredentials_ReturnsOk()
    {
        var dto = new LoginDto { Email = "test@mail.com", Password = "1234" };
        var expectedResponse = new LoginResponseDto
        {
            Token = Guid.NewGuid(),
            User = new UserSummaryDto { Email = dto.Email }
        };

        _sessionServiceMock
            .Setup(s => s.Login(dto))
            .Returns(expectedResponse);

        var result = _controller.Login(dto);

        var okResult = result as OkObjectResult;

        Assert.IsNotNull(okResult);
        Assert.AreEqual(200, okResult.StatusCode);

        var returnedResponse = okResult.Value as LoginResponseDto;
        Assert.IsNotNull(returnedResponse);
        Assert.AreEqual(expectedResponse.User.Email, returnedResponse.User.Email);
    }

    [TestMethod]
    public void Login_CallsSessionService()
    {
        var dto = new LoginDto { Email = "test@mail.com", Password = "1234" };
        var expectedResponse = new LoginResponseDto
        {
            Token = Guid.NewGuid(),
            User = new UserSummaryDto { Email = dto.Email }
        };

        _sessionServiceMock
            .Setup(s => s.Login(dto))
            .Returns(expectedResponse);

        _controller.Login(dto);

        _sessionServiceMock.Verify(s => s.Login(dto), Times.Once);
    }

    #endregion

    #region LogoutTests

    [TestMethod]
    public void Logout_ShouldCallService_AndReturnNoContent()
    {
        var token = Guid.NewGuid();
        var sessionService = new Mock<ISessionService>();
        var controller = new SessionController(sessionService.Object);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        controller.Request.Headers["Authorization"] = token.ToString();

        var result = controller.Logout();

        sessionService.Verify(s => s.Logout(token), Times.Once);

        Assert.IsInstanceOfType(result, typeof(NoContentResult));
    }

    #endregion
}