using Domain;
using DTOs.OrderDTOs;

namespace BusinessLogic.Mappers;

public static class OrderMapper
{
    public static CreateOrderResponseDto ToCreateOrderResponseDto(Order order)
    {
        return new CreateOrderResponseDto
        {
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            Subtotal = order.Subtotal,
            ShippingCost = order.ShippingCost,
            Total = order.Total
        };
    }

    public static OrderStatusChangeResponseDto ToStatusChangeResponseDto(Order order)
    {
        return new OrderStatusChangeResponseDto
        {
            OrderNumber = order.OrderNumber,
            Status = order.Status,
            ChangedAt = DateTime.Now
        };
    }

    public static OrderSummaryDto ToSummaryDto(Order order)
    {
        return new OrderSummaryDto
        {
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerEmail = order.Customer.Email,
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            Total = order.Total,
            TotalProducts = order.Lines.Sum(l => l.Quantity)
        };
    }

    public static OrderDetailResponseDto ToDetailDto(Order order)
    {
        return new OrderDetailResponseDto
        {
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            CustomerEmail = order.Customer.Email,
            CustomerName = $"{order.Customer.Name} {order.Customer.Surname}",
            CreatedAt = order.CreatedAt,
            Status = order.Status,
            ShippingType = order.ShippingType.Name,
            ShippingCost = order.ShippingCost,
            Street = order.DeliveryAddress.Street,
            DoorNumber = order.DeliveryAddress.DoorNumber,
            Apartment = order.DeliveryAddress.Apartment,
            Subtotal = order.Subtotal,
            Total = order.Total,
            Lines = order.Lines.Select(line => new OrderLineResponseDto
            {
                ProductId = line.ProductId,
                ProductName = line.Product.Name,
                Quantity = line.Quantity,
                UnitPrice = line.UnitPrice,
                Discount = line.Discount,
                Subtotal = line.Subtotal
            }).ToList()
        };
    }
}