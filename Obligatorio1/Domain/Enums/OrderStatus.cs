namespace Domain;

public enum OrderStatus
{
    Pending,
    Prepared,
    Cancelled,
    InTransit,
    Delivered,
    NotDelivered,
    Delayed
}