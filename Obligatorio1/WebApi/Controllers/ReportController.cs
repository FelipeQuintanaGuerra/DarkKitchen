using Domain.Enums;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportController(IReportService reportService) : ControllerBase
{
    [HttpGet("top-products")]
    [AuthorizationFilter([])]
    public IActionResult GetTopProducts(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var products = reportService.GetTopProducts(from, to);
        return Ok(products);
    }

    [HttpGet("sales")]
    [AuthorizationFilter([])]
    public IActionResult GetSalesReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to)
    {
        var report = reportService.GetSalesReport(from, to);
        return Ok(report);
    }
}
