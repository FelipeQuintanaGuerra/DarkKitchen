namespace Domain;

public class PendingOrderState : OrderState
{
    public PendingOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.Pending;

    public override void Prepare()
    {
        Order.ChangeState(new PreparedOrderState(Order));
    }

    public override void Cancel()
    {
        Order.ChangeState(new CancelledOrderState(Order));
    }

    public override void Delay()
    {
        Order.ChangeState(new DelayedOrderState(Order));
    }
}