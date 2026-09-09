using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class AuditLogger
{
    public Guid Id { get; set; }
    public String Username { get; set; }
    public String Action { get; set; }
    public DateTime Time { get; set; }
    public String? EntityName { get; set; }
    public Guid? EntityId { get; set; }
    public String? Description { get; set; }

    [ExcludeFromCodeCoverage]
    private AuditLogger() { }

    public AuditLogger(String username, String action, String entityName, Guid entityId, String description)
    {
        ValidateUser(username);
        ValidateAction(action);
        ValidateEntityName(entityName);
        ValidateDescription(description);

        Id = Guid.NewGuid();
        Username = username;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        Description = description;
        Time = DateTime.Now;
    }

    private static void ValidateAction(string action)
    {
        if (String.IsNullOrEmpty(action))
            throw new ArgumentException("La accion no puede estar vacia");
    }

    private static void ValidateUser(String username)
    {
        if (String.IsNullOrEmpty(username))
            throw new ArgumentException("El nombre del usuario no puede estar vacio");
    }

    private static void ValidateEntityName(String entityName)
    {
        if (String.IsNullOrEmpty(entityName))
            throw new ArgumentException("El nombre de la entidad no puede estar vacio");
    }

    private static void ValidateDescription(String description)
    {
        if (String.IsNullOrEmpty(description))
            throw new ArgumentException("La descripcion no puede estar vacia");
    }
}