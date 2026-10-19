using Domain;
using Repository;

namespace RepositoriesTests;

[TestClass]
public class ProductRepositoryTests : RepositoryTestBase
{
    private ProductRepository _productRepository = null!;

    [TestInitialize]
    public void InitializeRepository()
    {
        _productRepository = new ProductRepository(Context);
    }

    [TestMethod]
    public void GetBy_WhenProductExists_ReturnsProductWithCategoryCommercialLineAndPictures()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        var product = new Product(
            "CODE01",
            "ProductOne",
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image1.jpg", 100) },
            100
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _productRepository.GetBy(p => p.Id == product.Id);

        Assert.IsNotNull(result);
        Assert.IsNotNull(result.Category);
        Assert.IsNotNull(result.CommercialLine);
        Assert.AreEqual(1, result.Pictures.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenProductsExist_ReturnsProductsWithCategoryCommercialLineAndPictures()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        var product = new Product(
            "CODE01",
            "ProductOne",
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image1.jpg", 100) },
            100
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _productRepository.GetAllBy(p => p.Id == product.Id);

        Assert.AreEqual(1, result.Count);
        Assert.IsNotNull(result[0].Category);
        Assert.IsNotNull(result[0].CommercialLine);
        Assert.AreEqual(1, result[0].Pictures.Count);
    }

    [TestMethod]
    public void GetBy_WhenProductDoesNotExist_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = _productRepository.GetBy(p => p.Id == nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAllBy_WhenNoProductsMatch_ReturnsEmptyList()
    {
        var nonExistentId = Guid.NewGuid();

        var result = _productRepository.GetAllBy(p => p.Id == nonExistentId);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void ReplacePictures_WhenProductHasOldPictures_ReplacesThemWithNewPictures()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        var product = new Product(
            "CODE01",
            "ProductOne",
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture>
            {
                new ProductPicture("old1.jpg", 100),
                new ProductPicture("old2.jpg", 100)
            },
            100
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.SaveChanges();

        var newPictures = new List<ProductPicture>
        {
            new ProductPicture("new1.jpg", 100)
        };

        _productRepository.ReplacePictures(product.Id, newPictures);

        Context.ChangeTracker.Clear();

        var result = _productRepository.GetBy(p => p.Id == product.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Pictures.Count);
        Assert.AreEqual("new1.jpg", result.Pictures[0].FileName);
    }

    [TestMethod]
    public void ReplacePictures_WhenCalled_RemovesOldPicturesFromDatabase()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        var oldPicture = new ProductPicture("old1.jpg", 100);

        var product = new Product(
            "CODE01",
            "ProductOne",
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture> { oldPicture },
            100
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.SaveChanges();

        var oldPictureId = oldPicture.Id;

        var newPictures = new List<ProductPicture>
        {
            new ProductPicture("new1.jpg", 100)
        };

        _productRepository.ReplacePictures(product.Id, newPictures);

        var oldPictureStillExists = Context.ProductPictures
            .Any(p => p.Id == oldPictureId);

        Assert.IsFalse(oldPictureStillExists);
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsOne_ReturnsOnlyOneProduct()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);

        Context.Products.Add(CreateProduct("CODE01", "ProductOne", category, commercialLine));
        Context.Products.Add(CreateProduct("CODE02", "ProductTwo", category, commercialLine));
        Context.Products.Add(CreateProduct("CODE03", "ProductThree", category, commercialLine));

        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        var result = _productRepository.GetAllBy(
            p => p.Name.Contains("Product"),
            page: 1,
            pageSize: 1);

        Assert.AreEqual(1, result.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenSecondPageIsRequested_SkipsFirstPage()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);

        Context.Products.Add(CreateProduct("CODE01", "ProductOne", category, commercialLine));
        Context.Products.Add(CreateProduct("CODE02", "ProductTwo", category, commercialLine));
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        var firstPage = _productRepository.GetAllBy(
            p => p.Name.Contains("Product"),
            page: 1,
            pageSize: 1);

        var secondPage = _productRepository.GetAllBy(
            p => p.Name.Contains("Product"),
            page: 2,
            pageSize: 1);

        Assert.AreEqual(1, firstPage.Count);
        Assert.AreEqual(1, secondPage.Count);
        Assert.AreNotEqual(firstPage[0].Id, secondPage[0].Id);
    }

    [TestMethod]
    public void GetAllBy_WhenPageIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _productRepository.GetAllBy(
                p => true,
                page: 0,
                pageSize: 20));
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _productRepository.GetAllBy(
                p => true,
                page: 1,
                pageSize: 0));
    }

    private static Product CreateProduct(
        string code,
        string name,
        Category category,
        CommercialLine commercialLine)
    {
        return new Product(
            code,
            name,
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture($"{code}.jpg", 100) },
            100
        );
    }
}