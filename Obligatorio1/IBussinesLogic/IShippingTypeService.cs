using DTOs.ShippingTypeDTOs;

namespace IBussinesLogic;

public interface IShippingTypeService
{
    ShippingTypeResponseDto Create(CreateShippingTypeRequestDto request);
    ShippingTypeResponseDto Update(Guid id, UpdateShippingTypeRequestDto request);
    List<ShippingTypeResponseDto> GetAll();
    void Delete(Guid id);
}