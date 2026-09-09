using Domain.Enums;
using DTOs.ShippingTypeDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/shipping-types")]
public class ShippingTypeController : ControllerBase
{
    private readonly IShippingTypeService _shippingTypeService;

    public ShippingTypeController(IShippingTypeService shippingTypeService)
    {
        _shippingTypeService = shippingTypeService;
    }

    [HttpPost]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult Create([FromBody] CreateShippingTypeRequestDto request)
    {
        var response = _shippingTypeService.Create(request);

        return StatusCode(201, response);
    }

    [HttpPut("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult Update(Guid id, [FromBody] UpdateShippingTypeRequestDto request)
    {
        var response = _shippingTypeService.Update(id, request);

        return Ok(response);
    }

    [HttpGet]
    [AuthorizationFilter([])]
    public IActionResult GetAll()
    {
        var response = _shippingTypeService.GetAll();

        return Ok(response);
    }

    [HttpDelete("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult Delete(Guid id)
    {
        _shippingTypeService.Delete(id);

        return NoContent();
    }
}