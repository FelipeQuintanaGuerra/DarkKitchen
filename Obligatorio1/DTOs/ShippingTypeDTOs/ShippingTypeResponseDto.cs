namespace DTOs.ShippingTypeDTOs;

public class ShippingTypeResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Cost { get; set; }
    public bool IsDefault { get; set; }
}