using Domain;
using DTOs.ShippingTypeDTOs;

namespace BusinessLogic.Mappers;

public static class ShippingTypeMapper
{
    public static ShippingTypeResponseDto ToResponseDto(ShippingType shippingType)
    {
        return new ShippingTypeResponseDto
        {
            Id = shippingType.Id,
            Name = shippingType.Name,
            Cost = shippingType.Cost,
            IsDefault = shippingType.IsDefault
        };
    }

    public static List<ShippingTypeResponseDto> ToResponseDtoList(List<ShippingType> shippingTypes)
    {
        return shippingTypes
            .Select(ToResponseDto)
            .ToList();
    }
}