using Domain.Enums;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/importers")]
public class ImporterController(IImporterService importerService) : ControllerBase
{
    [HttpGet]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult GetImporterNames()
    {
        var names = importerService.GetImporterNames();
        return Ok(names);
    }

    [HttpPost("import")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult ImportProducts([FromQuery] string importerName, [FromQuery] string filePath)
    {
        var count = importerService.ImportProducts(importerName, filePath);
        return Ok(new { imported = count });
    }
}
