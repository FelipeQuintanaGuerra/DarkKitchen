using BusinessLogic.Mappers;
using Domain;
using DTOs.ShippingTypeDTOs;
using IBussinesLogic;
using IRepository;

namespace BusinessLogic;

public class ShippingTypeService : IShippingTypeService
{
    private const int DefaultPage = 1;
    private const int DefaultPageSize = 100;

    private readonly IGenericRepository<ShippingType> _shippingTypeRepository;
    private readonly IGenericRepository<Order> _orderRepository;

    public ShippingTypeService(IGenericRepository<ShippingType> shippingTypeRepository, IGenericRepository<Order> orderRepository)
    {
        _shippingTypeRepository = shippingTypeRepository;
        _orderRepository = orderRepository;
    }

    public ShippingTypeResponseDto Create(CreateShippingTypeRequestDto request)
    {
        EnsureNameIsAvailable(request.Name);

        var shippingType = new ShippingType(request.Name, request.Cost);

        _shippingTypeRepository.Add(shippingType);

        return ShippingTypeMapper.ToResponseDto(shippingType);
    }

    public ShippingTypeResponseDto Update(Guid id, UpdateShippingTypeRequestDto request)
    {
        var shippingType = GetShippingTypeOrThrow(id);

        EnsureNameIsAvailableForUpdate(request.Name, id);

        shippingType.Update(request.Name, request.Cost);

        _shippingTypeRepository.Update(shippingType);

        return ShippingTypeMapper.ToResponseDto(shippingType);
    }

    public List<ShippingTypeResponseDto> GetAll()
    {
        var shippingTypes = _shippingTypeRepository.GetAllBy(
            s => true,
            DefaultPage,
            DefaultPageSize);

        return ShippingTypeMapper.ToResponseDtoList(shippingTypes);
    }

    public void Delete(Guid id)
    {
        var shippingType = GetShippingTypeOrThrow(id);

        if (shippingType.IsDefault)
            throw new InvalidOperationException("No se puede eliminar un tipo de envío predeterminado.");

        var orderUsingShippingType = _orderRepository.GetBy(o => o.ShippingTypeId == id);
        if (orderUsingShippingType != null)
            throw new InvalidOperationException("No se puede eliminar este tipo de envío porque está asociado a una o más órdenes.");

        _shippingTypeRepository.Delete(shippingType);
    }

    private ShippingType GetShippingTypeOrThrow(Guid id)
    {
        var shippingType = _shippingTypeRepository.GetBy(s => s.Id == id);

        if (shippingType == null)
            throw new KeyNotFoundException("El tipo de envío no existe.");

        return shippingType;
    }

    private void EnsureNameIsAvailable(string name)
    {
        var existingShippingType = _shippingTypeRepository.GetBy(s => s.Name == name);

        if (existingShippingType != null)
            throw new ConflictException("Ya existe un tipo de envío con ese nombre.");
    }

    private void EnsureNameIsAvailableForUpdate(string name, Guid currentShippingTypeId)
    {
        var existingShippingType = _shippingTypeRepository.GetBy(
            s => s.Name == name && s.Id != currentShippingTypeId);

        if (existingShippingType != null)
            throw new ConflictException("Ya existe un tipo de envío con ese nombre.");
    }
}