namespace Domain;

public class CommercialLine
{
    public Guid Id { get; set; }
    public string Name { get; set; }

    public CommercialLine(string name)
    {
        ValidateName(name);

        Name = name;
        Id = Guid.NewGuid();
    }

    private static void ValidateName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre de la línea comercial no puede ser nulo.");
    }
}