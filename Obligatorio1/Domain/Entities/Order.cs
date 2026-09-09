using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Order
{
    private const decimal TaxMultiplier = 1.22m;

    private static readonly Dictionary<OrderStatus, Func<Order, OrderState>> StateFactory = new()
    {
        { OrderStatus.Pending, o => new PendingOrderState(o) },
        { OrderStatus.Prepared, o => new PreparedOrderState(o) },
        { OrderStatus.Cancelled, o => new CancelledOrderState(o) },
        { OrderStatus.InTransit, o => new InTransitOrderState(o) },
        { OrderStatus.Delivered, o => new DeliveredOrderState(o) },
        { OrderStatus.NotDelivered, o => new NotDeliveredOrderState(o) },
        { OrderStatus.Delayed, o => new DelayedOrderState(o) }
    };

    private OrderState _state;
    private List<OrderLine> _lines = new List<OrderLine>();

    public Guid OrderNumber { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public OrderStatus Status { get; private set; }
    public Guid CustomerId { get; private set; }
    public User Customer { get; private set; } = null!;
    public ShippingType ShippingType { get; private set; }
    public Guid ShippingTypeId { get; private set; }
    public decimal ShippingCost { get; private set; }
    public DeliveryAddress DeliveryAddress { get; private set; }
    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public decimal Subtotal { get; private set; }
    public decimal Total { get; private set; }

    [ExcludeFromCodeCoverage]
    private Order() { }

    public Order(Guid customerId, ShippingType shippingType, DeliveryAddress deliveryAddress)
    {
        if (shippingType == null)
            throw new ArgumentNullException(nameof(shippingType), "El tipo de envío es obligatorio.");

        if (customerId == Guid.Empty)
            throw new ArgumentException("El identificador del cliente es obligatorio.");

        if (deliveryAddress == null)
            throw new ArgumentNullException(nameof(deliveryAddress), "La dirección de entrega es obligatoria.");

        OrderNumber = Guid.NewGuid();
        CreatedAt = DateTime.Now;
        CustomerId = customerId;
        DeliveryAddress = deliveryAddress;
        SetShippingType(shippingType);
        ChangeState(new PendingOrderState(this));
    }

    public void Prepare()
    {
        _state.Prepare();
    }

    public void Cancel()
    {
        _state.Cancel();
    }

    public void MarkAsInTransit()
    {
        _state.MarkAsInTransit();
    }

    public void Deliver()
    {
        _state.Deliver();
    }

    public void MarkAsNotDelivered()
    {
        _state.MarkAsNotDelivered();
    }

    public void Delay()
    {
        _state.Delay();
    }

    public void AddLine(OrderLine line)
    {
        if (line == null)
            throw new ArgumentNullException(nameof(line), "La línea del pedido es obligatoria.");

        _lines.Add(line);
    }

    public void CalculateTotals()
    {
        if (!_lines.Any())
            throw new InvalidOperationException("El pedido debe tener al menos una línea.");

        Subtotal = _lines.Sum(l => l.Subtotal);
        ShippingCost = ShippingType.Cost;
        Total = (Subtotal + ShippingCost) * TaxMultiplier;
    }

    public void RestoreState(OrderStatus status)
    {
        if (!StateFactory.TryGetValue(status, out var factory))
            throw new ArgumentException("Estado inválido.");

        _state = factory(this);
        Status = status;
    }

    internal void RestoreShippingType(ShippingType shippingType)
    {
        ShippingType = shippingType;
    }

    private void SetShippingType(ShippingType shippingType)
    {
        ShippingType = shippingType;
        ShippingTypeId = shippingType.Id;
    }

    internal void ChangeState(OrderState newState)
    {
        _state = newState;
        Status = newState.Status;
    }
}