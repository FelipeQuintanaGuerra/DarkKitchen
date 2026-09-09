using Domain;
namespace DTOs.OrderDTOs;

public class CreateOrderResponseDto
{
    public Guid OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Subtotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal Total { get; set; }
}