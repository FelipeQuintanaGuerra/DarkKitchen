using Domain;

namespace DTOs.OrderDTOs;

public class OrderSummaryDto
{
    public Guid OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerEmail { get; set; }
    public DateTime CreatedAt { get; set; }
    public OrderStatus Status { get; set; }
    public decimal Total { get; set; }
    public int TotalProducts { get; set; }
}