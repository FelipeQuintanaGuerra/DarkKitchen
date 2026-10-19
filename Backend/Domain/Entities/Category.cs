namespace Domain;

public class Category
{
    private string _name;

    public Guid Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            ValidateName(value);
            _name = value;
        }
    }

    public Category(string name)
    {
        Name = name;
        Id = Guid.NewGuid();
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name),
                "El nombre de la categoría no puede ser nulo.");
    }
}