using DTOs.ReportDTOs;

namespace IBussinesLogic;

public interface IReportService
{
    List<TopProductDto> GetTopProducts(DateTime from, DateTime to);
    List<MonthlySalesDto> GetSalesReport(DateTime from, DateTime to);
}