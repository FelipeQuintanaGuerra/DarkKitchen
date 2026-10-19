using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;

namespace Domain;


public class Product
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; private set; }
    public string Description { get; private set; }
    public decimal Price { get; private set; }
    public CommercialLine CommercialLine { get; private set; }
    public Category Category { get; private set; }
    public List<ProductPicture> Pictures { get; set; }
    public bool IsActive { get; set; }

    [ExcludeFromCodeCoverage]
    private Product() { }

    public Product(string code, string name, string description, CommercialLine commercialLine, Category category, List<ProductPicture> pictures, decimal price)
    {
        ValidateCode(code);
        ValidateName(name);
        ValidateDescription(description);
        ValidateCommercialLine(commercialLine);
        ValidateCategory(category);
        ValidatePictures(pictures);
        ValidatePrice(price);

        Code = code;
        Id = Guid.NewGuid();
        Name = name;
        Description = description;
        CommercialLine = commercialLine;
        Category = category;
        Pictures = pictures;
        Price = price;
        IsActive = true;
    }

    public void SetPictures(List<ProductPicture> newPictures)
    {
        ValidatePictures(newPictures);
        Pictures.Clear();
        foreach (var pic in newPictures)
        {
            Pictures.Add(pic);
        }
    }
    public void AddPicture(ProductPicture picture)
    {
        if (picture == null)
            throw new ArgumentNullException("La imagen no puede ser null.");
        if (Pictures.Count >= 3)
            throw new ArgumentException("El producto no puede tener más de 3 imágenes.");
        Pictures.Add(picture);
    }

    public void RemovePicture(ProductPicture picture)
    {
        if (picture == null)
            throw new ArgumentNullException("La imagen no puede ser null.");
        if (!Pictures.Contains(picture))
            throw new ArgumentException("La imagen no pertenece al producto.");
        if (Pictures.Count == 1)
            throw new ArgumentException("El producto no puede quedar sin imagenes");

        Pictures.Remove(picture);
    }

    public void Update(string name, string description, decimal price, CommercialLine commercialLine, Category category)
    {
        ValidateName(name);
        ValidateDescription(description);
        ValidatePrice(price);
        ValidateCommercialLine(commercialLine);
        ValidateCategory(category);

        Name = name;
        Description = description;
        Price = price;
        CommercialLine = commercialLine;
        Category = category;
    }

    public void ChangeActivation()
    {
        IsActive = !IsActive;
    }


    private static void ValidateCode(string code)
    {
        if (code == null)
            throw new ArgumentNullException(nameof(code));

        if (code.Length < 5 || code.Length > 20)
            throw new ArgumentException("El código debe tener entre 5 y 20 caracteres");

        if (!Regex.IsMatch(code, "^[a-zA-Z0-9]+$"))
            throw new ArgumentException("El código debe ser alfanumérico.");
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException("El nombre no puede estar vacío.");
        if (name.Length < 10 || name.Length > 50)
            throw new ArgumentException("El nombre del producto debe estar entre 10 y 50 caracteres");
    }

    private static void ValidateDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentNullException("El nombre no puede estar vacío.");
        if (description.Length < 20 || description.Length > 500)
            throw new ArgumentException("La descripción del producto debe de estar entre 20 y 500 caracteres");
    }

    private static void ValidatePrice(decimal price)
    {
        if (price == null || price <= 0)
            throw new ArgumentException("El precio debe ser mayor a 0.");
    }

    private static void ValidateCommercialLine(CommercialLine commercialLine)
    {
        if (commercialLine == null)
            throw new ArgumentNullException("La línea comercial no puede estar vacía.");
    }

    private static void ValidateCategory(Category category)
    {
        if (category == null)
            throw new ArgumentNullException("La categoría no puede estar vacía.");
    }

    private static void ValidatePictures(List<ProductPicture> pictures)
    {
        if (pictures == null)
            throw new ArgumentNullException("La lista de imágenes no existe.");

        if (pictures.Count < 1)
            throw new ArgumentException("El producto debe tener al menos una imagen.");

        if (pictures.Count > 3)
            throw new ArgumentException("El producto puede tener hasta 3 imágenes.");
    }
}