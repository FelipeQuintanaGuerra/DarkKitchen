namespace Domain;

public abstract class OrderState
{
    protected Order Order { get; }

    protected OrderState(Order order)
    {
        Order = order;
    }

    public abstract OrderStatus Status { get; }

    public virtual void Prepare()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede ser preparado.");
    }

    public virtual void Cancel()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede ser cancelado.");
    }

    public virtual void MarkAsInTransit()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede pasar a 'En tránsito'.");
    }

    public virtual void Deliver()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede ser entregado.");
    }

    public virtual void MarkAsNotDelivered()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede marcarse como 'No entregado'.");
    }

    public virtual void Delay()
    {
        throw new ConflictException($"El pedido en estado '{Status}' no puede ser demorado.");
    }
}