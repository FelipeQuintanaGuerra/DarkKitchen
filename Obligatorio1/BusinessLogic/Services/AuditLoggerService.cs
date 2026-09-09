using Domain;
using DTOs.AuditDTOs;
using IBussinesLogic;
using IRepository;

namespace BusinessLogic;

public class AuditLoggerService : IAuditLoggerService
{
    private readonly IGenericRepository<AuditLogger> _auditLoggerRepository;

    public AuditLoggerService(IGenericRepository<AuditLogger> auditLoggerRepository)
    {
        _auditLoggerRepository = auditLoggerRepository;
    }

    public Task LogAsync(CreateLogDto entry)
    {
        var description = BuildDescription(entry);

        var auditLogger = new AuditLogger(
            entry.Username,
            entry.Action,
            entry.EntityName,
            entry.EntityId,
            description
        );

        _auditLoggerRepository.Add(auditLogger);

        return Task.CompletedTask;
    }

    public List<AuditLogResponseDto> Search(SearchAuditRequestDto dto)
    {
        if (dto.From >= dto.To)
            throw new ArgumentException("La fecha desde debe ser menor que la fecha hasta.");

        var results = _auditLoggerRepository.GetAllBy(
            a => a.Time >= dto.From &&
                 a.Time <= dto.To &&
                 (string.IsNullOrEmpty(dto.EntityName) || a.EntityName == dto.EntityName) &&
                 (dto.EntityId == Guid.Empty || a.EntityId == dto.EntityId)
        );

        return results.Select(a => new AuditLogResponseDto
        {
            Id = a.Id,
            Time = a.Time,
            EntityName = a.EntityName,
            EntityId = a.EntityId,
            Description = a.Description,
            Username = a.Username
        }).ToList();
    }

    private static string BuildDescription(CreateLogDto entry)
    {
        var dateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var entityName = entry.EntityName.ToLower();

        return entry.Action switch
        {
            "Alta" =>
                $"Se creó {entityName} de nombre {entry.EntityDisplayName}, ID {entry.EntityId}.",

            "Modificación" =>
                $"Se modificó {entityName} de nombre {entry.EntityDisplayName}, ID {entry.EntityId}.",

            "Eliminación" =>
                $"Se eliminó {entityName} de nombre {entry.EntityDisplayName}, ID {entry.EntityId}.",

            "Asociación" =>
                $"Se asoció un producto a la promoción {entry.EntityDisplayName}, ID {entry.EntityId}.",

            "Desasociación" =>
                $"Se removió un producto de la promoción {entry.EntityDisplayName}, ID {entry.EntityId}.",

            _ =>
                $"Se realizó la acción {entry.Action} sobre {entityName} de nombre {entry.EntityDisplayName}, ID {entry.EntityId}."
        };
    }
}