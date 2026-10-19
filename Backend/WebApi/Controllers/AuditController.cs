using DTOs.AuditDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;
using Domain.Enums;

namespace WebApi.Controllers;

[ApiController]
[Route("api/audit")]
public class AuditController(IAuditLoggerService auditLoggerService) : ControllerBase
{
    [HttpGet]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult Search([FromQuery] SearchAuditRequestDto dto)
    {
        var results = auditLoggerService.Search(dto);
        return Ok(results);
    }
}
