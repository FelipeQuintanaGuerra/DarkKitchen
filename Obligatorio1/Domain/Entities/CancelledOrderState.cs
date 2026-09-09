namespace Domain;

public class CancelledOrderState : OrderState
{
    public CancelledOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.Cancelled;
}