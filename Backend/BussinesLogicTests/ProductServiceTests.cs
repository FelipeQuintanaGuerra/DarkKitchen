using BusinessLogic;
using DTOs.ProductsDTOs;
using IRepository;
using Moq;
using IBussinesLogic;
using Domain;
using System.Linq.Expressions;
using DTOs.AuditDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace BussinesLogicTests;

[TestClass]
public class ProductServiceTests
{
    private Mock<IProductRepository> _mockProductRepo = null!;
    private Mock<IGenericRepository<CommercialLine>> _mockCommercialLineRepo = null!;
    private Mock<IGenericRepository<Category>> _mockCategoryRepo = null!;
    private Mock<IImageStorageService> _mockImageStorageService = null!;
    private Mock<ISessionRepository> _mockSessionRepository = null!;
    private Mock<IAuditLoggerService> _mockAuditLoggerService = null!;
    private IProductService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _mockImageStorageService = new Mock<IImageStorageService>(MockBehavior.Strict);
        _mockProductRepo = new Mock<IProductRepository>(MockBehavior.Strict);
        _mockCommercialLineRepo = new Mock<IGenericRepository<CommercialLine>>(MockBehavior.Strict);
        _mockCategoryRepo = new Mock<IGenericRepository<Category>>(MockBehavior.Strict);
        _mockSessionRepository = new Mock<ISessionRepository>(MockBehavior.Strict);
        _mockAuditLoggerService = new Mock<IAuditLoggerService>(MockBehavior.Strict);

        _mockImageStorageService
            .Setup(s => s.SaveProductImages(It.IsAny<List<IFormFile>>()))
            .Returns(new List<string> { "products/foto.jpg" });

        _service = new ProductService(
            _mockProductRepo.Object,
            _mockCommercialLineRepo.Object,
            _mockCategoryRepo.Object,
            _mockSessionRepository.Object,
            _mockImageStorageService.Object,
            _mockAuditLoggerService.Object
        );
    }

    private static IFormFile CreateFakeImage(string fileName = "foto.jpg", int sizeInKb = 100)
    {
        var bytes = new byte[sizeInKb * 1024];
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, stream.Length, "Images", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
    }

    private void SetupProductAdd()
    {
        _mockProductRepo
            .Setup(r => r.Add(It.IsAny<Product>()));
    }

    private void SetupProductUpdate()
    {
        _mockProductRepo
            .Setup(r => r.Update(It.IsAny<Product>()));
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

    private void SetupReplacePictures()
    {
        _mockProductRepo
            .Setup(r => r.ReplacePictures(
                It.IsAny<Guid>(),
                It.IsAny<List<ProductPicture>>()));
    }

    private Guid SetupValidSession()
    {
        var user = new User(
            "Admin",
            "Tester",
            "admin@test.com",
            "099123456",
            "Password123aaaaa!",
            Domain.Enums.Role.Admin,
            true);

        var session = new Session(user);

        _mockSessionRepository
            .Setup(r => r.GetByToken(session.Token))
            .Returns(session);

        return session.Token;
    }

    private void SetupAuditLog()
    {
        _mockAuditLoggerService
            .Setup(a => a.LogAsync(It.IsAny<CreateLogDto>()))
            .Returns(Task.CompletedTask);
    }

    [TestMethod]
    public void CreateProduct_ValidRequest_SavesAndReturnsDto()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductAdd();
        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.CreateProduct(request, token);

        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
        Assert.AreEqual(request.Code, result.Code);
    }

    [TestMethod]
    public void CreateProduct_ValidRequest_AuditsProductCreation()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductAdd();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.CreateProduct(request, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Alta" &&
                    dto.EntityName == "PRODUCTO" &&
                    dto.EntityDisplayName == request.Name &&
                    dto.EntityId != Guid.Empty)),
            Times.Once);
    }

    [TestMethod]
    public void CreateProduct_WhenCommercialLineDoesNotExist_CreatesIt()
    {
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductAdd();

        SetupCommercialLineAdd();
        var token = SetupValidSession();
        SetupAuditLog();

        _service.CreateProduct(request, token);

        _mockCommercialLineRepo.Verify(r => r.Add(It.IsAny<CommercialLine>()), Times.Once);
        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [TestMethod]
    public void CreateProduct_WhenCategoryDoesNotExist_CreatesIt()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = "Fritos",
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupProductAdd();

        SetupCategoryAdd();
        var token = SetupValidSession();
        SetupAuditLog();

        _service.CreateProduct(request, token);

        _mockCategoryRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);
        _mockProductRepo.Verify(r => r.Add(It.IsAny<Product>()), Times.Once);
    }

    [TestMethod]
    public void CreateProduct_NullRequest_ThrowsException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => _service.CreateProduct(null!, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreateProduct_WhenSessionDoesNotExist_ThrowsUnauthorizedAccessException()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductAdd();

        _mockSessionRepository
            .Setup(r => r.GetByToken(It.IsAny<Guid>()))
            .Returns((Session?)null);

        Assert.ThrowsException<UnauthorizedAccessException>(
            () => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void ChangeProductActivation_WhenProductBecomesActive_AuditsModification()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var product = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        product.ChangeActivation();

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupProductUpdate();

        CreateLogDto? capturedDto = null;

        _mockAuditLoggerService
            .Setup(a => a.LogAsync(It.IsAny<CreateLogDto>()))
            .Callback<CreateLogDto>(dto => capturedDto = dto)
            .Returns(Task.CompletedTask);

        var token = SetupValidSession();

        _service.ChangeProductActivation(product.Id, token);

        Assert.IsNotNull(capturedDto);
        Assert.AreEqual("Modificación", capturedDto.Action);
    }

    [TestMethod]
    public void CreateProduct_DuplicateCode_ThrowsException()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        var existingProduct = new Product(
            request.Code,
            request.Name,
            request.Description,
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            request.Price
        );

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        Assert.ThrowsException<ConflictException>(
            () => _service.CreateProduct(request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreateProduct_ValidImages_CallsImageStorageService()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductAdd();
        var token = SetupValidSession();
        SetupAuditLog();

        _service.CreateProduct(request, token);

        _mockImageStorageService.Verify(
            s => s.SaveProductImages(request.Images),
            Times.Once
        );
    }

    [TestMethod]
    public void CreateProduct_ImageWithInvalidExtension_ThrowsException()
    {
        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile> { CreateFakeImage("foto.png") }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(new CommercialLine("Minutas clasicas"));

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new Category("Fritos"));

        Assert.ThrowsException<ArgumentException>(() => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void CreateProduct_ImageOver500Kb_ThrowsException()
    {
        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile> { CreateFakeImage("foto.jpg", 501) }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(new CommercialLine("Minutas clasicas"));

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new Category("Fritos"));

        Assert.ThrowsException<ArgumentException>(() => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void UpdateProduct_ValidRequest_UpdatesAndReturnsDto()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductUpdate();

        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.UpdateProduct(existingProduct.Id, request, token);

        _mockProductRepo.Verify(r => r.Update(existingProduct), Times.Once);
        Assert.AreEqual(request.Name, result.Name);
    }

    [TestMethod]
    public void UpdateProduct_ValidRequest_AuditsProductModification()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductUpdate();
        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.UpdateProduct(existingProduct.Id, request, token);

        _mockAuditLoggerService.Verify(a => a.LogAsync(
                It.Is<CreateLogDto>(dto =>
                    dto.Username == "admin@test.com" &&
                    dto.Action == "Modificación" &&
                    dto.EntityName == "PRODUCTO" &&
                    dto.EntityId == existingProduct.Id &&
                    dto.EntityDisplayName == request.Name)),
            Times.Once);
    }

    [TestMethod]
    public void UpdateProduct_NullRequest_ThrowsException()
    {
        Assert.ThrowsException<ArgumentNullException>(
            () => _service.UpdateProduct(Guid.NewGuid(), null!, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdateProduct_NonExistingProduct_ThrowsException()
    {
        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.UpdateProduct(Guid.NewGuid(), request, Guid.NewGuid())
        );
    }

    [TestMethod]
    public void UpdateProduct_NonExistingCommercialLine_CreatesCommercialLine()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = "Linea inexistente",
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns((CommercialLine?)null);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupCommercialLineAdd();

        // Act
        SetupProductUpdate();

        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.UpdateProduct(existingProduct.Id, request, token);

        // Assert
        _mockCommercialLineRepo.Verify(r => r.Add(It.IsAny<CommercialLine>()), Times.Once);

        Assert.AreEqual("Linea inexistente", existingProduct.CommercialLine.Name);
    }

    [TestMethod]
    public void UpdateProduct_NonExistingCategory_CreatesCategory()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = commercialLine.Name,
            CategoryName = "Categoria inexistente",
            Images = new List<IFormFile> { CreateFakeImage() }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns((Category?)null);

        SetupCategoryAdd();

        SetupProductUpdate();

        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.UpdateProduct(existingProduct.Id, request, token);

        _mockCategoryRepo.Verify(r => r.Add(It.IsAny<Category>()), Times.Once);

        Assert.AreEqual("Categoria inexistente", existingProduct.Category.Name);
    }

    [TestMethod]
    public void UpdateProduct_WithNewImages_ReplacesPictures()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("products/old.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage("new.jpg") }
        };

        _mockImageStorageService
            .Setup(s => s.SaveProductImages(request.Images))
            .Returns(new List<string> { "products/new.jpg" });

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductUpdate();

        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.UpdateProduct(existingProduct.Id, request, token);

        Assert.AreEqual(1, existingProduct.Pictures.Count);
        Assert.AreEqual("products/new.jpg", existingProduct.Pictures[0].FileName);
    }

    [TestMethod]
    public void UpdateProduct_WithoutImages_KeepsExistingPictures()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("products/old.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            Price = 200,
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile>()
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        _service.UpdateProduct(existingProduct.Id, request, token);

        Assert.AreEqual(1, existingProduct.Pictures.Count);
        Assert.AreEqual("products/old.jpg", existingProduct.Pictures[0].FileName);

        _mockImageStorageService.Verify(
            s => s.SaveProductImages(It.IsAny<List<IFormFile>>()),
            Times.Never
        );
    }

    [TestMethod]
    public void SearchProducts_ReturnsFilteredProducts()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var product = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        _mockProductRepo
            .Setup(r => r.GetAllBy(It.IsAny<Expression<Func<Product, bool>>>(), 1, 20))
            .Returns(new List<Product> { product });

        var result = _service.SearchProducts(new SearchProductRequestDto());

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(product.Code, result[0].Code);
    }

    [TestMethod]
    public void ChangeProductActivation_ExistingProduct_ChangesActivation()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var product = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupProductUpdate();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.ChangeProductActivation(product.Id, token);

        Assert.IsFalse(result.IsActive);
        _mockProductRepo.Verify(r => r.Update(product), Times.Once);
    }

    [TestMethod]
    public void ChangeProductActivation_ExistingProduct_AuditsProductModification()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var product = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(product);

        SetupProductUpdate();

        CreateLogDto? capturedDto = null;

        _mockAuditLoggerService
            .Setup(a => a.LogAsync(It.IsAny<CreateLogDto>()))
            .Callback<CreateLogDto>(dto => capturedDto = dto)
            .Returns(Task.CompletedTask);

        var token = SetupValidSession();

        _service.ChangeProductActivation(product.Id, token);

        Assert.IsNotNull(capturedDto);
        Assert.AreEqual("admin@test.com", capturedDto.Username);
        Assert.AreEqual("Eliminación", capturedDto.Action);
        Assert.AreEqual("PRODUCTO", capturedDto.EntityName);
        Assert.AreEqual(product.Id, capturedDto.EntityId);
        Assert.AreEqual(product.Name, capturedDto.EntityDisplayName);
    }

    [TestMethod]
    public void ChangeProductActivation_NonExistingProduct_ThrowsException()
    {
        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        Assert.ThrowsException<KeyNotFoundException>(
            () => _service.ChangeProductActivation(Guid.NewGuid(), Guid.NewGuid())
        );
    }

    [TestMethod]
    public void CreateProduct_ValidRequest_ReturnsDtoWithCorrectPrice()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() },
            Price = 250
        };

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        SetupProductAdd();
        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.CreateProduct(request, token);

        Assert.AreEqual(250, result.Price);
    }

    [TestMethod]
    public void UpdateProduct_ValidRequest_UpdatesPrice()
    {
        var commercialLine = new CommercialLine("Minutas clasicas");
        var category = new Category("Fritos");

        var existingProduct = new Product(
            "PROD1",
            "Producto valido nombre",
            "Descripcion valida que cumple el minimo requerido",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("foto.jpg", 100) },
            100
        );

        var request = new UpdateProductRequestDto
        {
            Name = "Nombre actualizado ok",
            Description = "Descripcion actualizada que cumple el minimo",
            CommercialLineName = commercialLine.Name,
            CategoryName = category.Name,
            Images = new List<IFormFile> { CreateFakeImage() },
            Price = 350
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(existingProduct);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(commercialLine);

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(category);

        SetupProductUpdate();

        SetupReplacePictures();

        var token = SetupValidSession();
        SetupAuditLog();

        var result = _service.UpdateProduct(existingProduct.Id, request, token);

        Assert.AreEqual(350, result.Price);
    }

    [TestMethod]
    public void CreateProduct_NullImages_ThrowsArgumentNullException()
    {
        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = null!
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(new CommercialLine("Minutas clasicas"));

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new Category("Fritos"));

        Assert.ThrowsException<ArgumentNullException>(() => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void CreateProduct_EmptyImages_ThrowsArgumentException()
    {
        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile>()
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(new CommercialLine("Minutas clasicas"));

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new Category("Fritos"));

        Assert.ThrowsException<ArgumentException>(() => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void CreateProduct_MoreThanThreeImages_ThrowsArgumentException()
    {
        var request = new CreateProductRequestDto
        {
            Code = "PROD1",
            Name = "Producto valido nombre",
            Price = 100,
            Description = "Descripcion valida que cumple el minimo requerido",
            CommercialLineName = "Minutas clasicas",
            CategoryName = "Fritos",
            Images = new List<IFormFile>
            {
                CreateFakeImage(),
                CreateFakeImage(),
                CreateFakeImage(),
                CreateFakeImage()
            }
        };

        _mockProductRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Product?)null);

        _mockCommercialLineRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<CommercialLine, bool>>>()))
            .Returns(new CommercialLine("Minutas clasicas"));

        _mockCategoryRepo
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Category, bool>>>()))
            .Returns(new Category("Fritos"));

        Assert.ThrowsException<ArgumentException>(() => _service.CreateProduct(request, Guid.NewGuid()));
    }

    [TestMethod]
    public void SearchProducts_WhenPaginationIsProvided_UsesCorrectPageAndPageSize()
    {
        var dto = new SearchProductRequestDto
        {
            Page = 2,
            PageSize = 5
        };

        _mockProductRepo
            .Setup(r => r.GetAllBy(
                It.IsAny<Expression<Func<Product, bool>>>(),
                dto.Page,
                dto.PageSize))
            .Returns(new List<Product>());

        _service.SearchProducts(dto);

        _mockProductRepo.Verify(r => r.GetAllBy(
                It.IsAny<Expression<Func<Product, bool>>>(),
                2,
                5),
            Times.Once);
    }
}