namespace DTOs.ReportDTOs;


public class MonthlySalesDto
{
    public string Period { get; set; }
    public List<CustomerSalesDto> Customers { get; set; }
    public decimal Total { get; set; }
}