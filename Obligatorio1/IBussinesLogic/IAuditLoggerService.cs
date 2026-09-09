using DTOs.AuditDTOs;

namespace IBussinesLogic;

public interface IAuditLoggerService
{
    Task LogAsync(CreateLogDto entry);
    List<AuditLogResponseDto> Search(SearchAuditRequestDto dto);
}