using Domain;
using DTOs.ReportDTOs;

namespace BusinessLogic.Mappers;

public static class ReportMapper
{
    public static TopProductDto ToTopProductDto(Product product, int quantity)
    {
        return new TopProductDto
        {
            Code = product.Code,
            Name = product.Name,
            Quantity = quantity,
            Images = product.Pictures.Select(p => p.FileName).ToList()
        };
    }

    public static MonthlySalesDto ToMonthlySalesDto(
        string period,
        List<CustomerSalesDto> customers)
    {
        return new MonthlySalesDto
        {
            Period = period,
            Customers = customers,
            Total = customers.Sum(c => c.Total)
        };
    }

    public static CustomerSalesDto ToCustomerSalesDto(User customer, decimal total)
    {
        return new CustomerSalesDto
        {
            CustomerName = $"{customer.Name} {customer.Surname}",
            Total = total
        };
    }
}
