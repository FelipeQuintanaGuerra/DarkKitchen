using Domain;
using Repository;

namespace RepositoriesTests;

[TestClass]
public class PromotionRepositoryTests : RepositoryTestBase
{
    private PromotionRepository _promotionRepository = null!;

    [TestInitialize]
    public void InitializeRepository()
    {
        _promotionRepository = new PromotionRepository(Context);
    }

    [TestMethod]
    public void GetBy_WhenPromotionExists_ReturnsPromotionWithProducts()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("Commercial Line");
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(10);

        var product = new Product(
            "CODE01",
            "Product One",
            "Product description one",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture("image1.jpg", 100) },
            100
        );

        var promotion = new Promotion(
            "PromoTest",
            startDate,
            endDate,
            10,
            commercialLine,
            new List<Product> { product }
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.Promotions.Add(promotion);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _promotionRepository.GetBy(p => p.Id == promotion.Id);

        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Products.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenPromotionsExist_ReturnsPromotionsWithProducts()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        var product = new Product(
        "CODE01",
        "ProductOne",
        "This is a valid product description", // ✔️ OK
        commercialLine,
        category,
        new List<ProductPicture> { new ProductPicture("img.jpg", 100) },
        100
    );
        var startDate = DateTime.Now.AddDays(1);
        var endDate = DateTime.Now.AddDays(10);

        var promotion = new Promotion(
            "PromoTest",
            startDate,
            endDate,
            10,
            commercialLine,
            new List<Product> { product }
        );

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);
        Context.Products.Add(product);
        Context.Promotions.Add(promotion);
        Context.SaveChanges();

        Context.ChangeTracker.Clear();

        var result = _promotionRepository.GetAllBy(p => p.Id == promotion.Id);

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(1, result[0].Products.Count);
    }

    [TestMethod]
    public void GetBy_WhenPromotionDoesNotExist_ReturnsNull()
    {
        var nonExistentId = Guid.NewGuid();

        var result = _promotionRepository.GetBy(p => p.Id == nonExistentId);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void GetAllBy_WhenNoPromotionsMatch_ReturnsEmptyList()
    {
        var nonExistentId = Guid.NewGuid();

        var result = _promotionRepository.GetAllBy(p => p.Id == nonExistentId);

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsOne_ReturnsOnlyOnePromotion()
    {
        var category = new Category("Category");
        var commercialLine = new CommercialLine("CommercialLine");

        Context.Categories.Add(category);
        Context.CommercialLines.Add(commercialLine);

        Context.Promotions.Add(CreatePromotion("PromoOne", category, commercialLine));
        Context.Promotions.Add(CreatePromotion("PromoTwo", category, commercialLine));
        Context.Promotions.Add(CreatePromotion("PromoThree", category, commercialLine));

        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        var result = _promotionRepository.GetAllBy(
            p => p.Name.Contains("Promo"),
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

        Context.Promotions.Add(CreatePromotion("PromoOne", category, commercialLine));
        Context.Promotions.Add(CreatePromotion("PromoTwo", category, commercialLine));

        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        var firstPage = _promotionRepository.GetAllBy(
            p => p.Name.Contains("Promo"),
            page: 1,
            pageSize: 1);

        var secondPage = _promotionRepository.GetAllBy(
            p => p.Name.Contains("Promo"),
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
            _promotionRepository.GetAllBy(
                p => true,
                page: 0,
                pageSize: 20));
    }

    [TestMethod]
    public void GetAllBy_WhenPageSizeIsLessThanOne_ThrowsArgumentException()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            _promotionRepository.GetAllBy(
                p => true,
                page: 1,
                pageSize: 0));
    }

    private static Promotion CreatePromotion(
        string name,
        Category category,
        CommercialLine commercialLine)
    {
        var product = new Product(
            $"{name}CODE",
            $"{name} Product",
            "This is a valid product description",
            commercialLine,
            category,
            new List<ProductPicture> { new ProductPicture($"{name}.jpg", 100) },
            100
        );

        return new Promotion(
            name,
            DateTime.Now.AddDays(1),
            DateTime.Now.AddDays(10),
            10,
            commercialLine,
            new List<Product> { product }
        );
    }
}