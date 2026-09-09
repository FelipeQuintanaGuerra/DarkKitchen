namespace Domain;

public class NotDeliveredOrderState : OrderState
{
    public NotDeliveredOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.NotDelivered;
}