using Domain.Enums;
using IRepository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace WebApi.Authorization;

public class AuthorizationFilter : IAuthorizationFilter
{
    private readonly HashSet<string> _roles;
    private readonly ISessionRepository _sessionRepository;

    public AuthorizationFilter(Role[] roles, ISessionRepository sessionRepository)
    {
        if (roles is null)
        {
            throw new ArgumentNullException("Error especificando los roles permitidos.", nameof(roles));
        }

        _roles = roles
            .Select(r => r.ToString())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        _sessionRepository = sessionRepository;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var tokenString = context.HttpContext.Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenString) || !Guid.TryParse(tokenString, out var token))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var session = _sessionRepository.GetByToken(token);

        if (session == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (_roles.Count == 0)
        {
            return;
        }

        var userRole = session.User.Role.ToString();
        var isAllowed = _roles.Contains(userRole, StringComparer.OrdinalIgnoreCase);

        if (!isAllowed)
        {
            context.Result = new ObjectResult(new { error = "FORBIDDEN", message = "El usuario loggeado no tiene permisos para acceder a esta función." }) { StatusCode = 403 };
        }
    }
}