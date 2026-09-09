using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class Promotion
{
    public Guid Id { get; set; }
    public string Name { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public int Discount { get; private set; }
    public CommercialLine CommercialLine { get; private set; }
    public List<Product> Products { get; set; }

    [ExcludeFromCodeCoverage]
    private Promotion() { }

    public Promotion(string name, DateTime startDate, DateTime endDate, int discount, CommercialLine commercialLine, List<Product> products)
    {
        ValidateName(name);
        ValidateStartDate(startDate);
        ValidateEndDate(startDate, endDate);
        ValidateDiscount(discount);
        ValidateCommercialLine(commercialLine);

        Id = Guid.NewGuid();
        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Discount = discount;
        CommercialLine = commercialLine;

        ValidateProducts(products);
        Products = products;
    }

    public void Update(string name, DateTime startDate, DateTime endDate, int discount)
    {
        ValidateName(name);
        ValidateEndDate(startDate, endDate);
        ValidateDiscount(discount);
        ValidateStartDate(startDate);

        Name = name;
        StartDate = startDate;
        EndDate = endDate;
        Discount = discount;
    }

    public void AddProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "El producto no puede ser vacío.");
        if (product.CommercialLine != CommercialLine)
            throw new ArgumentException("El producto no pertenece a la misma línea comercial.");
        if (Products.Contains(product))
            throw new ArgumentException("El producto ya está en la promoción.");

        Products.Add(product);
    }

    public void RemoveProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product), "El producto no puede ser vacío.");
        if (!Products.Contains(product))
            throw new ArgumentException("El producto no es parte de la promoción.");

        Products.Remove(product);
    }


    private void ValidateProducts(List<Product> products)
    {
        if (products == null)
            throw new ArgumentNullException(nameof(products), "La lista de productos no puede ser vacía.");
        if (products.Any(p => p.CommercialLine != CommercialLine))
            throw new ArgumentException("Todos los productos deben pertenecer a la línea comercial de la promoción.");
    }

    private static void ValidateCommercialLine(CommercialLine commercialLine)
    {
        if (commercialLine == null)
            throw new ArgumentNullException(nameof(commercialLine), "La línea comercial no puede ser vacía.");
    }

    private static void ValidateDiscount(int discount)
    {
        if (discount <= 0)
            throw new ArgumentException("El descuento debe ser mayor a 0%.");
        if (discount >= 100)
            throw new ArgumentException("El descuento no puede ser mayor que 100%.");
    }

    private static void ValidateEndDate(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("La promoción no puede terminar antes de empezar.");
    }

    private static void ValidateStartDate(DateTime startDate)
    {
        if (startDate < DateTime.Today)
            throw new ArgumentException("La promoción no puede empezar en el pasado.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name), "El nombre no puede estar vacío.");
        if (name.Length < 5)
            throw new ArgumentException("El nombre debe tener al menos 5 caracteres.");
        if (name.Length > 25)
            throw new ArgumentException("El nombre no debe superar los 25 caracteres.");
        if (!Regex.IsMatch(name, "^[a-zA-Z0-9 ]+$"))
            throw new ArgumentException("El nombre debe ser alfanumérico.");
    }
}