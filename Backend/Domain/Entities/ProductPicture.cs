using System.Diagnostics.CodeAnalysis;

namespace Domain;

public class ProductPicture
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public int SizeInKb { get; set; }

    [ExcludeFromCodeCoverage]
    private ProductPicture() { }

    public ProductPicture(string fileName, int sizeInKb)
    {
        ValidateFileName(fileName);
        ValidateSizeInKb(sizeInKb);

        Id = Guid.NewGuid();
        FileName = fileName;
        SizeInKb = sizeInKb;
    }

    private static void ValidateSizeInKb(int sizeInKb)
    {
        if (sizeInKb > 500)
            throw new ArgumentException("La imagen no puede superar los 500 KB.");
    }

    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentNullException("El nombre de la imagen no puede estar vacío.");
        if (!fileName.EndsWith(".jpg"))
            throw new ArgumentException("La imagen debe estar en formato jpg.");
    }
}