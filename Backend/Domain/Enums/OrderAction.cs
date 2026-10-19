namespace Domain.Enums;

public enum OrderAction
{
    Prepare,
    Cancel,
    MarkAsInTransit,
    Deliver,
    MarkAsNotDelivered,
    Delay
}