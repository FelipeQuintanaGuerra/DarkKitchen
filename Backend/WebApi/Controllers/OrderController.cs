using Domain.Enums;
using DTOs.OrderDTOs;
using IBussinesLogic;
using Microsoft.AspNetCore.Mvc;
using WebApi.Authorization;

namespace WebApi.Controllers;

[ApiController]
[Route("api/orders")]
public class OrderController(IOrderService orderService) : ControllerBase
{
    [HttpPost]
    [AuthorizationFilter([Role.Client])]
    public IActionResult CreateOrder([FromBody] CreateOrderRequestDto dto)
    {
        var response = orderService.CreateOrder(dto);
        return StatusCode(201, response);
    }

    [HttpGet("{id}")]
    [AuthorizationFilter([Role.Dispatcher, Role.Admin])]
    public IActionResult GetOrderById(Guid id)
    {
        var order = orderService.GetOrderDetail(id);
        return Ok(order);
    }

    [HttpPatch("{id}")]
    [AuthorizationFilter([Role.Admin, Role.Dispatcher])]
    public IActionResult ExecuteAction(Guid id, [FromBody] OrderActionRequestDto request)
    {
        var tokenString = Request.Headers["Authorization"].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(tokenString) || !Guid.TryParse(tokenString, out var token))
        {
            return Unauthorized();
        }

        var response = orderService.ExecuteAction(id, request.Action, token);
        return Ok(response);
    }

    [HttpGet("customer")]
    [AuthorizationFilter([Role.Client])]
    public IActionResult GetCustomerOrders([FromQuery] SearchCustomerOrdersRequestDto dto)
    {
        var orders = orderService.GetCustomerOrders(dto);
        return Ok(orders);
    }

    [HttpGet("customerF")]
    [AuthorizationFilter([Role.Dispatcher, Role.Admin])]
    public IActionResult GetFilteredOrders([FromQuery] SearchFilteredOrdersRequestDto dto)
    {
        var orders = orderService.GetFilteredOrders(dto);
        return Ok(orders);
    }
}
