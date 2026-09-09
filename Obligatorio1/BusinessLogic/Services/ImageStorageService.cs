using IBussinesLogic;
using Microsoft.AspNetCore.Http;

namespace BusinessLogic;

public class ImageStorageService : IImageStorageService
{
    private readonly string _webRootPath;

    public ImageStorageService(string webRootPath)
    {
        _webRootPath = webRootPath;
    }

    public List<string> SaveProductImages(List<IFormFile> images)
    {
        var imageUrls = new List<string>();

        var folderPath = Path.Combine(
            _webRootPath,
            "images",
            "products"
        );

        Directory.CreateDirectory(folderPath);

        foreach (var image in images)
        {
            var fileName = $"{Guid.NewGuid()}.jpg";
            var fullPath = Path.Combine(folderPath, fileName);

            using var stream = new FileStream(fullPath, FileMode.Create);
            image.CopyTo(stream);

            imageUrls.Add(fileName);
        }

        return imageUrls;
    }
}