using Domain.Enums;
using DTOs.User;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/users")]
public class UserController(IUserService userService) : ControllerBase
{
    private Guid GetToken()
    {
        var tokenString = Request.Headers["Authorization"].FirstOrDefault();
        Guid.TryParse(tokenString, out var token);
        return token;
    }

    [HttpPost]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult Create([FromBody] UpdateUserDto dto)
    {
        var user = userService.Create(dto, GetToken());
        return CreatedAtAction(nameof(GetUserById), new { id = user.Id }, user);
    }

    [HttpPost("registrations")]
    public IActionResult Register([FromBody] RegisterUserDto dto)
    {
        var user = userService.Register(dto);
        return StatusCode(201, user);
    }


    [HttpGet]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult SearchBy(
        [FromQuery] string? name,
        [FromQuery] string? surname)

    {
        var users = userService.SearchBy(name, surname);
        return Ok(users);
    }

    [HttpGet("{id}")]
    [AuthorizationFilter([])]
    public IActionResult GetUserById(Guid id)
    {
        var user = userService.GetUserById(id);
        return Ok(user);
    }

    [HttpPut("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        dto.Id = id;
        userService.Update(dto, GetToken());
        return Ok(new { message = "Usuario actualizado exitosamente." });
    }

    [HttpDelete("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult DeleteUser(Guid id)
    {
        userService.Delete(id, GetToken());
        return NoContent();
    }
}