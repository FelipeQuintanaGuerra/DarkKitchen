namespace DTOs.OrderDTOs;

public class CreateOrderLineRequestDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}