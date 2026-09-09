namespace DTOs.OrderDTOs;

public class SearchCustomerOrdersRequestDto
{
    public Guid CustomerId { get; set; }

    public DateTime? From { get; set; }

    public DateTime? To { get; set; }

    public string? Status { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}