using System.Diagnostics.CodeAnalysis;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Authorization;

public class AuthorizationFilterAttribute : TypeFilterAttribute
{
    [ExcludeFromCodeCoverage]
    public AuthorizationFilterAttribute(Role[] roles)
        : base(typeof(AuthorizationFilter))
    {
        Arguments = [roles];
    }
}