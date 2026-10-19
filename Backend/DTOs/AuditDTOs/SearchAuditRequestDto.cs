namespace DTOs.AuditDTOs;

public class SearchAuditRequestDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public String EntityName { get; set; }
    public Guid EntityId { get; set; }
}
