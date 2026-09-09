namespace DTOs.AuditDTOs;

public class CreateLogDto
{
    public String Username { get; set; }
    public String Action { get; set; }
    public String EntityName { get; set; }
    public Guid EntityId { get; set; }
    public string EntityDisplayName { get; set; }
}