using Domain.Enums;
using DTOs.ProductsDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/products")]
public class ProductController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult CreateProduct([FromForm] CreateProductRequestDto dto)
    {
        var product = productService.CreateProduct(dto, GetToken());
        return StatusCode(201, product);
    }

    [HttpPut("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult UpdateProduct(Guid id, [FromForm] UpdateProductRequestDto dto)
    {
        var product = productService.UpdateProduct(id, dto, GetToken());
        return Ok(product);
    }

    [HttpGet]
    [AuthorizationFilter([Role.Admin, Role.Client])]
    public IActionResult SearchProducts([FromQuery] SearchProductRequestDto dto)
    {
        var products = productService.SearchProducts(dto);
        return Ok(products);
    }

    [HttpGet("{id}")]
    [AuthorizationFilter([Role.Admin, Role.Client])]
    public IActionResult GetProduct(Guid id)
    {
        var product = productService.GetProductById(id);
        return Ok(product);
    }

    [HttpPatch("{id}")]
    [AuthorizationFilter([Role.Admin])]
    public IActionResult ChangeProductActivation(Guid id)
    {
        var product = productService.ChangeProductActivation(id, GetToken());
        return Ok(product);
    }

    private Guid GetToken()
    {
        var tokenString = Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenString) || !Guid.TryParse(tokenString, out var token))
            throw new UnauthorizedAccessException("Token inválido.");

        return token;
    }
}
