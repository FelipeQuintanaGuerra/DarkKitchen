namespace DTOs.ReportDTOs;


public class TopProductDto
{
    public string Code { get; set; }
    public string Name { get; set; }
    public int Quantity { get; set; }
    public List<string> Images { get; set; }
}