using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using WebApi.Filters;

[TestClass]
public class ExceptionFilterTests
{
    private ExceptionFilter _filter;

    [TestInitialize]
    public void Setup()
    {
        _filter = new ExceptionFilter();
    }

    [TestMethod]
    public void OnException_ArgumentException_ReturnsBadRequest()
    {
        var context = CreateExceptionContext(new ArgumentException("error de prueba"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(400, result.StatusCode);
    }

    private ExceptionContext CreateExceptionContext(Exception exception)
    {
        var actionContext = new ActionContext(
            new DefaultHttpContext(),
            new Microsoft.AspNetCore.Routing.RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        return new ExceptionContext(actionContext, new List<IFilterMetadata>())
        {
            Exception = exception
        };
    }

    [TestMethod]
    public void OnException_InvalidOperationException_ReturnsUnprocessableEntity()
    {
        var context = CreateExceptionContext(new InvalidOperationException("operacion invalida"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(422, result.StatusCode);
    }

    [TestMethod]
    public void OnException_KeyNotFoundException_ReturnsNotFound()
    {
        var context = CreateExceptionContext(new KeyNotFoundException("no encontrado"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(404, result.StatusCode);
    }

    [TestMethod]
    public void OnException_UnauthorizedAccessException_ReturnsUnauthorized()
    {
        var context = CreateExceptionContext(new UnauthorizedAccessException("no autorizado"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(401, result.StatusCode);
    }

    [TestMethod]
    public void OnException_UnhandledException_ReturnsInternalServerError()
    {
        var context = CreateExceptionContext(new Exception("error inesperado"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(500, result.StatusCode);
    }

    [TestMethod]
    public void OnException_SetsExceptionHandledTrue()
    {
        var context = CreateExceptionContext(new Exception("error"));

        _filter.OnException(context);

        Assert.IsTrue(context.ExceptionHandled);
    }

    [TestMethod]
    public void OnException_ConflictException_ReturnsConflict()
    {
        var context = CreateExceptionContext(new ConflictException("conflicto"));

        _filter.OnException(context);

        var result = context.Result as ObjectResult;
        Assert.IsNotNull(result);
        Assert.AreEqual(409, result.StatusCode);
    }
}