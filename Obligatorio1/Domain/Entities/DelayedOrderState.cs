namespace Domain;

public class DelayedOrderState : OrderState
{
    public DelayedOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.Delayed;

    public override void Prepare()
    {
        Order.ChangeState(new PreparedOrderState(Order));
    }

    public override void Cancel()
    {
        Order.ChangeState(new CancelledOrderState(Order));
    }
}
