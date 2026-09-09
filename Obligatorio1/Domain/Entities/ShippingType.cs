namespace Domain;

public class ShippingType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public decimal Cost { get; private set; }
    public bool IsDefault { get; set; }

    private ShippingType() { }

    public ShippingType(string name, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del tipo de envío es obligatorio.");

        if (cost < 0)
            throw new ArgumentException("El costo del tipo de envío no puede ser negativo.");

        Id = Guid.NewGuid();
        Name = name;
        Cost = cost;
        IsDefault = false;
    }

    public void Update(string name, decimal cost)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del tipo de envío es obligatorio.");

        if (cost < 0)
            throw new ArgumentException("El costo del tipo de envío no puede ser negativo.");

        Name = name;
        Cost = cost;
    }
}