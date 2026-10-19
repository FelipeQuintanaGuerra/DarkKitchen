namespace DTOs.AuditDTOs;

public class AuditLogResponseDto
{
    public Guid Id { get; set; }
    public DateTime Time { get; set; }
    public String? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public String? Description { get; set; }
    public String Username { get; set; }
}
