using Moq;
using IBussinesLogic;
using BusinessLogic;
using BusinessLogic.Reflection;
using IImporters;
using IRepository;
using Domain;

namespace BussinesLogicTests;

[TestClass]
public class ImporterServiceTests
{
    private Mock<IAssemblyLoader> _mockLoader = null!;
    private Mock<IProductRepository> _mockProductRepo = null!;
    private Mock<IGenericRepository<CommercialLine>> _mockCommercialLineRepo = null!;
    private Mock<IGenericRepository<Category>> _mockCategoryRepo = null!;
    private Mock<IFileService> _mockFileService = null!;
    private ImporterService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockLoader = new Mock<IAssemblyLoader>(MockBehavior.Strict);
        _mockProductRepo = new Mock<IProductRepository>(MockBehavior.Strict);
        _mockCommercialLineRepo = new Mock<IGenericRepository<CommercialLine>>(MockBehavior.Strict);
        _mockCategoryRepo = new Mock<IGenericRepository<Category>>(MockBehavior.Strict);
        _mockFileService = new Mock<IFileService>(MockBehavior.Strict);

        _service = new ImporterService(
            _mockLoader.Object,
            _mockProductRepo.Object,
            _mockCommercialLineRepo.Object,
            _mockCategoryRepo.Object,
            _mockFileService.Object
        );
    }

    private void SetupNoExistingProduct()
    {
        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .Returns((Product?)null);
    }

    private void SetupProductAdd()
    {
        _mockProductRepo
            .Setup(r => r.Add(It.IsAny<Product>()));
    }

    private void SetupCommercialLineAdd()
    {
        _mockCommercialLineRepo
            .Setup(r => r.Add(It.IsAny<CommercialLine>()));
    }

    private void SetupCategoryAdd()
    {
        _mockCategoryRepo
            .Setup(r => r.Add(It.IsAny<Category>()));
    }

    private void SetupCopyFile()
    {
        _mockFileService
            .Setup(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>()));
    }

    [TestMethod]
    public void GetImporterNames_ReturnsNamesFromAssemblyLoader()
    {
        var expectedNames = new List<string> { "JSON Importer", "XML Importer" };

        _mockLoader
            .Setup(l => l.GetImporterNames())
            .Returns(expectedNames);

        var result = _service.GetImporterNames();

        CollectionAssert.AreEqual(expectedNames, result);
    }

    [TestMethod]
    public void ImportProducts_PersistsProductsToDatabase()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "PROD01TEST",
                Name = "Pizza Margarita Especial",
                Description = "Pizza clasica con tomate y mozzarella fresca",
                Price = 350m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "pizza1.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();
        SetupProductAdd();

        _mockFileService
            .Setup(f => f.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileService
            .Setup(f => f.GetFileSizeInKb(It.IsAny<string>()))
            .Returns(100);

        SetupCopyFile();

        var result = _service.ImportProducts("JSON Importer", "products.json");

        Assert.AreEqual(1, result);
        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        _mockCommercialLineRepo.Verify(r => r.Add(It.IsAny<CommercialLine>()), Times.Once);
        _mockCategoryRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
    }

    [TestMethod]
    public void ImportProducts_ReusesExistingCommercialLineAndCategory()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);
        var existingLine = new CommercialLine("Pizzas");
        var existingCategory = new Category("Comida");

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "PROD02TEST",
                Name = "Hamburguesa Doble Queso",
                Description = "Doble carne con queso cheddar fundido",
                Price = 420m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "burger1.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("file.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns(existingLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns(existingCategory);

        SetupProductAdd();

        _mockFileService
            .Setup(f => f.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileService
            .Setup(f => f.GetFileSizeInKb(It.IsAny<string>()))
            .Returns(100);

        SetupCopyFile();

        var result = _service.ImportProducts("JSON Importer", "file.json");

        Assert.AreEqual(1, result);
        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        _mockCommercialLineRepo.Verify(r => r.Add(It.IsAny<CommercialLine>()), Times.Never);
        _mockCategoryRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Never);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void ImportProducts_ThrowsWhenImporterNotFound()
    {
        _mockLoader
            .Setup(l => l.GetImporterByName("Fake Importer"))
            .Returns((IProductImporter?)null);

        _service.ImportProducts("Fake Importer", "file.json");
    }

    [TestMethod]
    public void ImportProducts_CopiesImagesToDestinationFolder()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "IMG01TEST",
                Name = "Pizza con Imagenes",
                Description = "Pizza para test de imagenes",
                Price = 300m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/pizza1.jpg", "/fotos/pizza2.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();
        SetupProductAdd();

        _mockFileService
            .Setup(f => f.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileService
            .Setup(f => f.GetFileSizeInKb(It.IsAny<string>()))
            .Returns(100);

        SetupCopyFile();

        _service.ImportProducts("JSON Importer", "products.json");

        _mockFileService.Verify(f => f.CopyFile(
                "/fotos/pizza1.jpg",
                It.Is<string>(s => s.EndsWith(".jpg"))),
            Times.Once);

        _mockFileService.Verify(f => f.CopyFile(
                "/fotos/pizza2.jpg",
                It.Is<string>(s => s.EndsWith(".jpg"))),
            Times.Once);
    }

    [TestMethod]
    public void ImportProducts_ThrowsArgumentException_WhenImageFileDoesNotExist()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "IMG02TEST",
                Name = "Pizza sin Imagen",
                Description = "Pizza con imagen faltante",
                Price = 250m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/noexiste.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();

        _mockFileService
            .Setup(f => f.FileExists("/fotos/noexiste.jpg"))
            .Returns(false);

        var exception = Assert.ThrowsException<ArgumentException>(() =>
            _service.ImportProducts("JSON Importer", "products.json")
        );

        Assert.AreEqual("El producto 'Pizza sin Imagen' no tiene imágenes válidas.", exception.Message);

        _mockFileService.Verify(f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod]
    public void ImportProducts_ThrowsArgumentException_WhenImageIsNotJpgFormat()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "IMG03TEST",
                Name = "Pizza con Imagen PNG",
                Description = "Pizza con imagen en formato incorrecto",
                Price = 300m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/pizza.png" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();

        _mockFileService
            .Setup(f => f.FileExists("/fotos/pizza.png"))
            .Returns(true);

        var exception = Assert.ThrowsException<ArgumentException>(() =>
            _service.ImportProducts("JSON Importer", "products.json")
        );

        Assert.AreEqual(
            "El producto 'Pizza con Imagen PNG' no tiene imágenes válidas.",
            exception.Message);

        _mockFileService.Verify(
            f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _mockProductRepo.Verify(
            r => r.Add(It.IsAny<Product>()),
            Times.Never);
    }

    [TestMethod]
    public void ImportProducts_ThrowsArgumentException_WhenProductCodeAlreadyExists()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "EXISTSCODE",
                Name = "Producto Duplicado Test",
                Description = "Producto con codigo que ya existe",
                Price = 300m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/pizza.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Product, bool>>>()))
            .Returns(new Product(
                "EXISTSCODE",
                "Producto Existente Ya",
                "Descripcion del producto existente",
                new CommercialLine("Pizzas"),
                new Category("Comida"),
                new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
                200m));

        var exception = Assert.ThrowsException<ArgumentException>(() =>
            _service.ImportProducts("JSON Importer", "products.json")
        );

        Assert.AreEqual("Ya existe un producto con código 'EXISTSCODE'.", exception.Message);

        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Never);
    }

    [TestMethod]
    public void ImportProducts_SavesImageWithUniqueFileName()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "GUID01TEST",
                Name = "Pizza Nombre Unico",
                Description = "Pizza para test de nombre unico",
                Price = 300m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/pizza1.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();
        SetupProductAdd();

        _mockFileService
            .Setup(f => f.FileExists(It.IsAny<string>()))
            .Returns(true);

        _mockFileService
            .Setup(f => f.GetFileSizeInKb(It.IsAny<string>()))
            .Returns(100);

        SetupCopyFile();

        _service.ImportProducts("JSON Importer", "products.json");

        _mockFileService.Verify(f => f.CopyFile(
                "/fotos/pizza1.jpg",
                It.Is<string>(s => !s.Contains("pizza1.jpg") && s.EndsWith(".jpg"))),
            Times.Once);
    }

    [TestMethod]
    public void ImportProducts_ThrowsArgumentException_WhenImageSizeExceeds500Kb()
    {
        var mockImporter = new Mock<IProductImporter>(MockBehavior.Strict);

        var importedProducts = new List<ImportedProduct>
        {
            new ImportedProduct
            {
                Code = "IMG04TEST",
                Name = "Pizza con Imagen Grande",
                Description = "Pizza con imagen que supera 500KB",
                Price = 300m,
                CommercialLineName = "Pizzas",
                CategoryName = "Comida",
                ImagePaths = new List<string> { "/fotos/pizza_grande.jpg" }
            }
        };

        mockImporter
            .Setup(i => i.GetProducts("products.json"))
            .Returns(importedProducts);

        _mockLoader
            .Setup(l => l.GetImporterByName("JSON Importer"))
            .Returns(mockImporter.Object);

        SetupNoExistingProduct();

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<System.Linq.Expressions.Expression<System.Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCommercialLineAdd();
        SetupCategoryAdd();

        _mockFileService
            .Setup(f => f.FileExists("/fotos/pizza_grande.jpg"))
            .Returns(true);

        _mockFileService
            .Setup(f => f.GetFileSizeInKb("/fotos/pizza_grande.jpg"))
            .Returns(501);

        var exception = Assert.ThrowsException<ArgumentException>(() =>
            _service.ImportProducts("JSON Importer", "products.json")
        );

        Assert.AreEqual(
            "El producto 'Pizza con Imagen Grande' no tiene imágenes válidas.",
            exception.Message);

        _mockFileService.Verify(
            f => f.CopyFile(It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);

        _mockProductRepo.Verify(
            r => r.Add(It.IsAny<Product>()),
            Times.Never);
    }
}
