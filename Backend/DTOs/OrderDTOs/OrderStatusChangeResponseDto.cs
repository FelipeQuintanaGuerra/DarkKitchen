using Domain;
namespace DTOs.OrderDTOs;

public class OrderStatusChangeResponseDto
{
    public Guid OrderNumber { get; set; }
    public OrderStatus Status { get; set; }
    public DateTime ChangedAt { get; set; }
}