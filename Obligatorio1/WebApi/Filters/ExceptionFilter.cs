using Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Filters;

public class ExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var (statusCode, errorCode) = context.Exception switch
        {
            ConflictException => (409, "CONFLICT"),
            ArgumentException => (400, "BAD_REQUEST"),
            InvalidOperationException => (422, "UNPROCESSABLE_ENTITY"),
            KeyNotFoundException => (404, "NOT_FOUND"),
            UnauthorizedAccessException => (401, "UNAUTHORIZED"),
            _ => (500, "INTERNAL_SERVER_ERROR")
        };

        context.Result = new ObjectResult(new
        {
            error = errorCode,
            message = context.Exception.Message
        })
        {
            StatusCode = statusCode
        };

        context.ExceptionHandled = true;
    }
}