using Domain;
namespace DTOs.OrderDTOs;

public class OrderDetailResponseDto
{
    public Guid OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerName { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public string ShippingType { get; set; }
    public decimal ShippingCost { get; set; }
    public string Street { get; set; }
    public string DoorNumber { get; set; }
    public string Apartment { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Total { get; set; }
    public List<OrderLineResponseDto> Lines { get; set; } = new();
}