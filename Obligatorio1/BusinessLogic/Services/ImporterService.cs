using BusinessLogic.Reflection;
using Domain;
using IBussinesLogic;
using IImporters;
using IRepository;

namespace BusinessLogic;

public class ImporterService : IImporterService
{
    private readonly IAssemblyLoader _assemblyLoader;
    private readonly IProductRepository _productRepository;
    private readonly IGenericRepository<CommercialLine> _commercialLineRepository;
    private readonly IGenericRepository<Category> _categoryRepository;
    private readonly IFileService _fileService;
    private readonly string _imagesDestinationPath;

    public ImporterService(
        IAssemblyLoader assemblyLoader,
        IProductRepository productRepository,
        IGenericRepository<CommercialLine> commercialLineRepository,
        IGenericRepository<Category> categoryRepository,
        IFileService fileService,
        string imagesDestinationPath = "wwwroot/images/products")
    {
        _assemblyLoader = assemblyLoader;
        _productRepository = productRepository;
        _commercialLineRepository = commercialLineRepository;
        _categoryRepository = categoryRepository;
        _fileService = fileService;
        _imagesDestinationPath = imagesDestinationPath;
    }

    public List<string> GetImporterNames()
    {
        return _assemblyLoader.GetImporterNames();
    }

    public int ImportProducts(string importerName, string filePath)
    {
        var importer = _assemblyLoader.GetImporterByName(importerName);

        if (importer == null)
            throw new Exception($"Importer '{importerName}' not found.");

        var importedProducts = importer.GetProducts(filePath);
        var importedCount = 0;

        foreach (var imported in importedProducts)
        {
            var existingProduct = _productRepository.GetBy(p => p.Code == imported.Code);
            if (existingProduct != null)
                throw new ArgumentException($"Ya existe un producto con código '{imported.Code}'.");

            var commercialLine = _commercialLineRepository.GetBy(cl => cl.Name == imported.CommercialLineName);
            if (commercialLine == null)
            {
                commercialLine = new CommercialLine(imported.CommercialLineName);
                _commercialLineRepository.Add(commercialLine);
            }

            var category = _categoryRepository.GetBy(c => c.Name == imported.CategoryName);
            if (category == null)
            {
                category = new Category(imported.CategoryName);
                _categoryRepository.Add(category);
            }

            var pictures = new List<ProductPicture>();
            foreach (var imagePath in imported.ImagePaths)
            {
                if (!_fileService.FileExists(imagePath))
                    continue;

                var fileName = Path.GetFileName(imagePath);
                if (!fileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
                    continue;

                var sizeInKb = _fileService.GetFileSizeInKb(imagePath);
                if (sizeInKb > 500)
                    continue;

                var uniqueFileName = $"{Guid.NewGuid()}.jpg";
                var destinationPath = Path.Combine(_imagesDestinationPath, uniqueFileName);
                _fileService.CopyFile(imagePath, destinationPath);
                pictures.Add(new ProductPicture(uniqueFileName, sizeInKb));
            }

            if (pictures.Count == 0)
                throw new ArgumentException($"El producto '{imported.Name}' no tiene imágenes válidas.");

            var product = new Product(
                imported.Code,
                imported.Name,
                imported.Description,
                commercialLine,
                category,
                pictures,
                imported.Price
            );

            _productRepository.Add(product);
            importedCount++;
        }

        return importedCount;
    }
}
