using Domain.Enums;
using DTOs.OrderDTOs;

namespace IBussinesLogic;

public interface IOrderService
{
    CreateOrderResponseDto CreateOrder(CreateOrderRequestDto request);
    OrderStatusChangeResponseDto PrepareOrder(Guid orderNumber);
    OrderStatusChangeResponseDto CancelOrder(Guid orderNumber);
    OrderStatusChangeResponseDto MarkOrderAsInTransit(Guid orderNumber);
    OrderStatusChangeResponseDto DeliverOrder(Guid orderNumber);
    OrderStatusChangeResponseDto MarkOrderAsNotDelivered(Guid orderNumber);
    OrderStatusChangeResponseDto DelayOrder(Guid orderNumber);
    OrderDetailResponseDto GetOrderDetail(Guid orderNumber);
    List<OrderSummaryDto> GetCustomerOrders(SearchCustomerOrdersRequestDto dto);
    List<OrderSummaryDto> GetFilteredOrders(SearchFilteredOrdersRequestDto dto);
    public OrderStatusChangeResponseDto ExecuteAction(Guid orderNumber, OrderAction action, Guid token);
}