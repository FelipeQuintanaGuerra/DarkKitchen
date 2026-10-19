namespace Domain;

public class DeliveredOrderState : OrderState
{
    public DeliveredOrderState(Order order) : base(order)
    {
    }

    public override OrderStatus Status => OrderStatus.Delivered;
}