using Microsoft.AspNetCore.Http;

namespace IBussinesLogic;

public interface IImageStorageService
{
    List<string> SaveProductImages(List<IFormFile> images);
}