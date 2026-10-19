namespace Domain;

public class InTransitOrderState : OrderState
{
    public InTransitOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.InTransit;

    public override void Deliver()
    {
        Order.ChangeState(new DeliveredOrderState(Order));
    }

    public override void MarkAsNotDelivered()
    {
        Order.ChangeState(new NotDeliveredOrderState(Order));
    }
}