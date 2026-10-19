using DTOs.User;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;


[ApiController]
[Route("api/session")]
public class SessionController(ISessionService sessionService) : ControllerBase
{
    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        var user = sessionService.Login(dto);
        return Ok(user);
    }

    [HttpDelete]
    public IActionResult Logout()
    {
        var tokenString = Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(tokenString) || !Guid.TryParse(tokenString, out var token))
            return Unauthorized();

        sessionService.Logout(token);
        return NoContent();
    }
}