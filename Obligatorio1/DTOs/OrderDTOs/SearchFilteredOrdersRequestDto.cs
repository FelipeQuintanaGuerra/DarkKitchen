namespace DTOs.OrderDTOs;

public class SearchFilteredOrdersRequestDto
{
    public DateTime From { get; set; }

    public DateTime To { get; set; }

    public string? Status { get; set; }

    public string? AddressText { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}