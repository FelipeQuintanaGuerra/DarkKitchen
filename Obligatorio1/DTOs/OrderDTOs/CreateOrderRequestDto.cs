namespace DTOs.OrderDTOs;

public class CreateOrderRequestDto
{
    public Guid CustomerId { get; set; }
    public Guid ShippingTypeId { get; set; }
    public string Street { get; set; }
    public string DoorNumber { get; set; }
    public string Apartment { get; set; }
    public List<CreateOrderLineRequestDto> Lines { get; set; } = new();
}