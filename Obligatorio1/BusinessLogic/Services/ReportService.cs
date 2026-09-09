using BusinessLogic.Mappers;
using Domain;
using DTOs.ReportDTOs;
using IBussinesLogic;
using IRepository;

namespace BusinessLogic;

public class ReportService : IReportService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IGenericRepository<Product> _productRepository;

    public ReportService(
        IOrderRepository orderRepository,
        IGenericRepository<Product> productRepository
    )
    {
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public List<TopProductDto> GetTopProducts(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);

        var orders = _orderRepository.GetAllUnpagedBy(o =>
            o.CreatedAt >= from && o.CreatedAt <= to
        );

        var grouped = orders
            .SelectMany(o => o.Lines)
            .Where(l => l.Product != null)
            .GroupBy(l => l.Product)
            .Select(g => new { Product = g.Key, Quantity = g.Sum(l => l.Quantity) })
            .OrderByDescending(x => x.Quantity)
            .Take(5)
            .ToList();

        return grouped
            .Select(item => ReportMapper.ToTopProductDto(item.Product, item.Quantity))
            .ToList();
    }

    public List<MonthlySalesDto> GetSalesReport(DateTime from, DateTime to)
    {
        ValidateDateRange(from, to);

        var orders = _orderRepository.GetAllUnpagedBy(o =>
            o.CreatedAt >= from && o.CreatedAt <= to
        );

        var result = orders
            .GroupBy(o => new { o.CreatedAt.Year, o.CreatedAt.Month })
            .OrderBy(g => g.Key.Year)
            .ThenBy(g => g.Key.Month)
            .Select(monthGroup =>
            {
                var customers = monthGroup
                    .GroupBy(o => o.CustomerId)
                    .Select(g =>
                        ReportMapper.ToCustomerSalesDto(g.First().Customer, g.Sum(o => o.Total))
                    )
                    .ToList();

                var period = $"{monthGroup.Key.Year}-{monthGroup.Key.Month:D2}";
                return ReportMapper.ToMonthlySalesDto(period, customers);
            })
            .ToList();

        return result;
    }

    private static void ValidateDateRange(DateTime from, DateTime to)
    {
        if (from > to)
            throw new ArgumentException("La fecha desde no puede ser mayor a la fecha hasta.");
    }
}
