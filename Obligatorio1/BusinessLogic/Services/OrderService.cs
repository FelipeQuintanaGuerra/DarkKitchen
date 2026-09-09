using BusinessLogic.Mappers;
using Domain;
using Domain.Enums;
using DTOs.OrderDTOs;
using IBussinesLogic;
using IRepository;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic;

public class OrderService : IOrderService
{
    private readonly IGenericRepository<User> _userRepository;
    private readonly IPromotionService _promotionService;
    private readonly IOrderRepository _orderRepository;
    private readonly IGenericRepository<ShippingType> _shippingTypeRepository;
    private readonly IProductRepository _productRepository;
    private readonly ISessionRepository _sessionRepository;

    public OrderService(
        IOrderRepository orderRepository,
        IGenericRepository<User> userRepository,
        IProductRepository productRepository,
        IPromotionService promotionService,
        ISessionRepository sessionRepository,
        IGenericRepository<ShippingType> shippingTypeRepository)
    {
        _orderRepository = orderRepository;
        _shippingTypeRepository = shippingTypeRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _promotionService = promotionService;
        _sessionRepository = sessionRepository;
    }

    public CreateOrderResponseDto CreateOrder(CreateOrderRequestDto request)
    {
        ValidateCreateOrderRequest(request);

        EnsureCustomerExists(request.CustomerId);
        EnsureProductsExistAndAreActive(request.Lines);

        var order = BuildOrder(request);

        AddLinesToOrder(order, request.Lines);

        order.CalculateTotals();

        _orderRepository.Add(order);

        return OrderMapper.ToCreateOrderResponseDto(order);
    }

    public OrderStatusChangeResponseDto PrepareOrder(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.Prepare();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderStatusChangeResponseDto CancelOrder(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.Cancel();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderStatusChangeResponseDto MarkOrderAsInTransit(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.MarkAsInTransit();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderStatusChangeResponseDto DeliverOrder(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.Deliver();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderStatusChangeResponseDto MarkOrderAsNotDelivered(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.MarkAsNotDelivered();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderStatusChangeResponseDto DelayOrder(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        order.Delay();
        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    public OrderDetailResponseDto GetOrderDetail(Guid orderNumber)
    {
        Order order = GetOrderOrThrow(orderNumber);

        return OrderMapper.ToDetailDto(order);
    }

    public List<OrderSummaryDto> GetCustomerOrders(SearchCustomerOrdersRequestDto dto)
    {
        OrderStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var orderStatus))
                throw new ArgumentException("Estado de pedido inválido.");

            parsedStatus = orderStatus;
        }

        var orders = _orderRepository.GetAllBy(
            o =>
                o.CustomerId == dto.CustomerId &&
                (!parsedStatus.HasValue || o.Status == parsedStatus.Value) &&
                (!dto.From.HasValue || o.CreatedAt >= dto.From.Value) &&
                (!dto.To.HasValue || o.CreatedAt <= dto.To.Value),
            dto.Page,
            dto.PageSize
        );

        return orders.Select(OrderMapper.ToSummaryDto).ToList();
    }

    public List<OrderSummaryDto> GetFilteredOrders(SearchFilteredOrdersRequestDto dto)
    {
        OrderStatus? parsedStatus = null;

        if (!string.IsNullOrWhiteSpace(dto.Status))
        {
            if (!Enum.TryParse<OrderStatus>(dto.Status, true, out var orderStatus))
                throw new ArgumentException("Estado de pedido inválido.");

            parsedStatus = orderStatus;
        }

        var orders = _orderRepository.GetAllBy(
            o =>
                o.CreatedAt >= dto.From &&
                o.CreatedAt <= dto.To &&
                (!parsedStatus.HasValue || o.Status == parsedStatus.Value) &&
                (string.IsNullOrWhiteSpace(dto.AddressText) ||
                 o.DeliveryAddress.Street.Contains(dto.AddressText) ||
                 o.DeliveryAddress.DoorNumber.Contains(dto.AddressText) ||
                 o.DeliveryAddress.Apartment.Contains(dto.AddressText)),
            dto.Page,
            dto.PageSize
        );

        return orders.Select(OrderMapper.ToSummaryDto).ToList();
    }

    private Order GetOrderOrThrow(Guid orderNumber)
    {
        Order? order = _orderRepository.GetBy(o => o.OrderNumber == orderNumber);

        if (order == null)
            throw new KeyNotFoundException("El pedido no existe.");

        return order;
    }

    private static void ValidateCreateOrderRequest(CreateOrderRequestDto request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request), "La solicitud de creación de pedido no puede ser nula.");

        if (request.Lines == null || !request.Lines.Any())
            throw new ArgumentException("El pedido debe tener al menos una línea.");
    }

    private void EnsureCustomerExists(Guid customerId)
    {
        var customer = _userRepository.GetBy(x => x.Id == customerId);

        if (customer == null)
            throw new KeyNotFoundException("El cliente no existe.");
    }

    private void EnsureProductsExistAndAreActive(IEnumerable<CreateOrderLineRequestDto> lines)
    {
        foreach (var line in lines)
        {
            var product = GetProductOrThrow(line.ProductId);

            if (!product.IsActive)
                throw new InvalidOperationException("El producto no está activo.");
        }
    }

    private Order BuildOrder(CreateOrderRequestDto request)
    {
        var shippingType = GetShippingTypeOrThrow(request.ShippingTypeId);
        var address = new DeliveryAddress(
            request.Street,
            request.DoorNumber,
            request.Apartment
        );

        return new Order(request.CustomerId, shippingType, address);
    }

    private ShippingType GetShippingTypeOrThrow(Guid shippingTypeId)
    {
        var shippingType = _shippingTypeRepository.GetBy(s => s.Id == shippingTypeId);

        if (shippingType == null)
            throw new KeyNotFoundException("El tipo de envío no existe.");

        return shippingType;
    }

    private void AddLinesToOrder(Order order, IEnumerable<CreateOrderLineRequestDto> lines)
    {
        foreach (var line in lines)
        {
            var product = GetProductOrThrow(line.ProductId);
            var discount = GetDiscountForProduct(product.Id);

            var orderLine = new OrderLine(
                product.Id,
                line.Quantity,
                product.Price,
                discount
            );

            order.AddLine(orderLine);
        }
    }

    private Product GetProductOrThrow(Guid productId)
    {
        var product = _productRepository.GetBy(p => p.Id == productId);

        if (product == null)
            throw new KeyNotFoundException("El producto no existe.");

        return product;
    }

    private decimal GetDiscountForProduct(Guid productId)
    {
        var promotion = _promotionService.GetBestPromotionForProduct(productId, DateTime.Now);

        return promotion != null ? promotion.Discount / 100m : 0;
    }
    public OrderStatusChangeResponseDto ExecuteAction(
        Guid orderNumber,
        OrderAction action,
        Guid token)
    {
        var currentUser = GetCurrentUser(token);

        ValidatePermissions(currentUser, action);

        var order = GetOrderOrThrow(orderNumber);

        //order.RestoreState(order.Status);

        if (!Actions.TryGetValue(action, out var operation))
        {
            throw new ArgumentException("Acción no válida.");
        }

        operation(order);

        _orderRepository.Update(order);

        return OrderMapper.ToStatusChangeResponseDto(order);
    }

    private static readonly Dictionary<OrderAction, Action<Order>> Actions = new()
    {
        { OrderAction.Prepare, o => o.Prepare() },
        { OrderAction.Cancel, o => o.Cancel() },
        { OrderAction.MarkAsInTransit, o => o.MarkAsInTransit() },
        { OrderAction.Deliver, o => o.Deliver() },
        { OrderAction.MarkAsNotDelivered, o => o.MarkAsNotDelivered() },
        { OrderAction.Delay, o => o.Delay() }
    };

    private User GetCurrentUser(Guid token)
    {
        var session = _sessionRepository.GetByToken(token);

        if (session == null)
            throw new UnauthorizedAccessException();

        return session.User;
    }

    private static void ValidatePermissions(
        User user,
        OrderAction action)
    {
        if (!AllowedRoles.TryGetValue(action, out var roles))
        {
            throw new ArgumentException("Acción inválida.");
        }

        if (!roles.Contains(user.Role))
        {
            throw new UnauthorizedAccessException();
        }
    }

    private static readonly Dictionary<OrderAction, Role[]> AllowedRoles = new()
    {
        { OrderAction.Cancel, [Role.Admin] },
        { OrderAction.Prepare, [Role.Admin, Role.Dispatcher] },
        { OrderAction.MarkAsInTransit, [Role.Dispatcher] },
        { OrderAction.Deliver, [Role.Dispatcher] },
        { OrderAction.MarkAsNotDelivered, [Role.Dispatcher] },
        { OrderAction.Delay, [Role.Dispatcher] }
    };
}