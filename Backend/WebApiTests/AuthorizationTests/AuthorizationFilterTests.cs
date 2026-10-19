using Domain;
using Domain.Enums;
using IRepository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using WebApi.Authorization;

namespace WebApiTests.AuthorizationTests;

[TestClass]
public class AuthorizationFilterTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void Constructor_WithNullRoles_ThrowsException()
    {
        var sessionRepo = new Mock<ISessionRepository>();
        new AuthorizationFilter(null, sessionRepo.Object);
    }

    [TestMethod]
    public void OnAuthorization_WithEmptyRoles_AllowsAction()
    {
        var user = new User("Admin", "Test", "admin@mail.com", "099000000", "ValidPassword!Ab7", Role.Admin);
        var session = new Session(user);

        var sessionRepo = new Mock<ISessionRepository>();
        sessionRepo.Setup(r => r.GetByToken(session.Token)).Returns(session);

        var filter = new AuthorizationFilter(Array.Empty<Role>(), sessionRepo.Object);
        var context = CreateContext(session.Token.ToString());

        filter.OnAuthorization(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public void OnAuthorization_UserWithAllowedRole_AllowsAccess()
    {
        var user = new User("Admin", "Test", "admin@mail.com", "099000000", "AdminPassword!Ab7", Role.Admin);
        var session = new Session(user);

        var sessionRepo = new Mock<ISessionRepository>();
        sessionRepo.Setup(r => r.GetByToken(session.Token)).Returns(session);

        var filter = new AuthorizationFilter(new[] { Role.Admin }, sessionRepo.Object);
        var context = CreateContext(session.Token.ToString());

        filter.OnAuthorization(context);

        Assert.IsNull(context.Result);
    }

    [TestMethod]
    public void OnAuthorization_UserWithDisallowedRole_DeniesAccess()
    {
        var user = new User("Client", "Test", "client@mail.com", "099000000", "ClientPassword!Ab7", Role.Client);
        var session = new Session(user);

        var sessionRepo = new Mock<ISessionRepository>();
        sessionRepo.Setup(r => r.GetByToken(session.Token)).Returns(session);

        var filter = new AuthorizationFilter(new[] { Role.Admin }, sessionRepo.Object);
        var context = CreateContext(session.Token.ToString());

        filter.OnAuthorization(context);

        Assert.IsNotNull(context.Result);
    }

    [TestMethod]
    public void OnAuthorization_NoSessionFound_DeniesAccess()
    {
        var token = Guid.NewGuid();
        var sessionRepo = new Mock<ISessionRepository>();
        sessionRepo.Setup(r => r.GetByToken(token)).Returns((Session?)null);

        var filter = new AuthorizationFilter(new[] { Role.Admin }, sessionRepo.Object);
        var context = CreateContext(token.ToString());

        filter.OnAuthorization(context);

        Assert.IsNotNull(context.Result);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }

    [TestMethod]
    public void OnAuthorization_NoAuthorizationHeader_DeniesAccess()
    {
        var sessionRepo = new Mock<ISessionRepository>();

        var filter = new AuthorizationFilter(new[] { Role.Admin }, sessionRepo.Object);
        var context = CreateContext(null);

        filter.OnAuthorization(context);

        Assert.IsNotNull(context.Result);
        Assert.IsInstanceOfType(context.Result, typeof(UnauthorizedResult));
    }

    private AuthorizationFilterContext CreateContext(string? token)
    {
        var httpContext = new DefaultHttpContext();
        if (token != null)
        {
            httpContext.Request.Headers["Authorization"] = token;
        }
        var actionContext = new ActionContext(httpContext, new Microsoft.AspNetCore.Routing.RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor());
        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }
}