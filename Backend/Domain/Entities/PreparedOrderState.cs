namespace Domain;

public class PreparedOrderState : OrderState
{
    public PreparedOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.Prepared;

    public override void MarkAsInTransit()
    {
        Order.ChangeState(new InTransitOrderState(Order));
    }
}