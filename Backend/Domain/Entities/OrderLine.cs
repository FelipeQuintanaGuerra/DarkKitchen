using Domain;

public class OrderLine
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public Product Product { get; private set; } = null!;
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal Discount { get; private set; }
    public decimal Subtotal => (UnitPrice * Quantity) * (1 - Discount);

    private OrderLine()
    {
    }

    public OrderLine(Guid productId, int quantity, decimal unitPrice, decimal discount)
    {
        if (productId == Guid.Empty)
            throw new ArgumentNullException("El identificador del producto no puede ser vacío.");

        if (quantity <= 0)
            throw new ArgumentException("La cantidad debe ser mayor a 0.");

        if (unitPrice <= 0)
            throw new ArgumentException("El precio unitario debe ser mayor a 0.");

        if (discount < 0 || discount > 1)
            throw new ArgumentException("El descuento debe estar entre 0 y 1.");

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Discount = discount;
    }
}