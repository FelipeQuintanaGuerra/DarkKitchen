using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class PromotionTests
{
    private string _name = "Promo123";
    private DateTime _startDate = DateTime.Now.AddDays(1);
    private DateTime _endDate;
    private int _discount = 10;
    private CommercialLine _commercialLine = new CommercialLine("Minutas clásicas");
    private Category _category = new Category("Fritos");
    private List<ProductPicture> _pictures = new List<ProductPicture>()
    {
        new ProductPicture("foto.jpg", 400)
    };
    private List<Product> _products;

    [TestInitialize]
    public void Setup()
    {
        _endDate = _startDate.AddDays(5);
        Product product1 = new Product("AXA25", "ProductoDePrueba1", "Descripcion larga y valida", _commercialLine, _category, _pictures, 11);
        Product product2 = new Product("ALF41", "ProductoDePueba2", "Descripcion larga y valida", _commercialLine, _category, _pictures, 11);

        _products = new List<Product> { product1, product2 };
    }

    #region  NameTests

    [TestMethod]
    public void NameCannotBeNullOrEmpty()
    {
        Assert.ThrowsException<ArgumentNullException>(() => new Promotion(null, _startDate, _endDate, _discount, _commercialLine, _products));
        Assert.ThrowsException<ArgumentNullException>(() => new Promotion("", _startDate, _endDate, _discount, _commercialLine, _products));
    }
    [TestMethod]
    public void NameMustBeAtLeast5Characters()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion("Ab1", _startDate, _endDate, _discount, _commercialLine, _products));
    }

    [TestMethod]
    public void NameMustBeAtMost25Characters()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(new string('a', 26), _startDate, _endDate, _discount, _commercialLine, _products));
    }
    [TestMethod]
    public void NameMustBeAlphanumeric()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion("Promo!!!", _startDate, _endDate, _discount, _commercialLine, _products));
    }
    #endregion

    #region  StartDateTests

    [TestMethod]
    public void StartDateMustBeInFuture()
    {
        var pastDate = DateTime.Now.AddDays(-1);

        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(_name, pastDate, _endDate, _discount, _commercialLine, _products));
    }

    #endregion

    #region EndDateTests

    [TestMethod]
    public void EndDateMustBeAfterStartDate()
    {
        var invalidEndDate = _startDate.AddDays(-1);

        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(_name, _startDate, invalidEndDate, _discount, _commercialLine, _products));
    }

    #endregion

    #region DiscountTests

    [TestMethod]
    public void DiscountMustBeGreaterThanZero()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(_name, _startDate, _endDate, 0, _commercialLine, _products));
    }

    [TestMethod]
    public void DiscountCannotBeGreaterThan100()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(_name, _startDate, _endDate, 101, _commercialLine, _products));
    }

    #endregion

    #region CommercialLineTests

    [TestMethod]
    public void CommercialLineCannotBeNull()
    {
        Assert.ThrowsException<ArgumentNullException>(() =>
            new Promotion(_name, _startDate, _endDate, _discount, null, _products));
    }

    #endregion

    #region ProductsTests

    [TestMethod]
    public void ProductsMustBelongToCommercialLine()
    {
        var otherLine = new CommercialLine("Otra linea");

        var invalidProduct = new Product("ZZZ99", "ProductoDePrueba3", "Descripcion larga y valida", otherLine, _category, _pictures, 111);

        var products = new List<Product> { invalidProduct };

        Assert.ThrowsException<ArgumentException>(() =>
            new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, products));
    }

    [TestMethod]
    public void AddValidProductWorks()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        var newProduct = new Product("NEW11", "ProductoDePrueba4", "Descripcion larga y valida", _commercialLine, _category, _pictures, 111);

        promotion.AddProduct(newProduct);

        Assert.IsTrue(promotion.Products.Contains(newProduct));
    }
    [TestMethod]
    public void AddProductFromDifferentCommercialLineThrows()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        var otherLine = new CommercialLine("Otra linea");

        var invalidProduct = new Product("BAD11", "ProductoDePrueba5", "Descripcion larga y valida", otherLine, _category, _pictures, 111);

        Assert.ThrowsException<ArgumentException>(() =>
            promotion.AddProduct(invalidProduct));
    }
    [TestMethod]
    public void AddNullProductThrows()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);
        Product nullProduct = null;

        Assert.ThrowsException<ArgumentNullException>(() =>
            promotion.AddProduct(nullProduct));
    }
    [TestMethod]
    public void AddProductAlreadyInPromotionThrows()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);


        Assert.ThrowsException<ArgumentException>(() =>
            promotion.AddProduct(_products[0]));
    }
    [TestMethod]
    public void RemovealidProductWorks()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        var deletedProduct = promotion.Products[0];

        promotion.RemoveProduct(deletedProduct);

        Assert.IsFalse(promotion.Products.Contains(deletedProduct));
    }
    [TestMethod]
    public void RemoveProductNotInPromotionThrows()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);
        var newProduct = new Product("BAD11", "ProductoDePrueba6", "Descripcion larga y valida", _commercialLine, _category, _pictures, 111);


        Assert.ThrowsException<ArgumentException>(() =>
            promotion.RemoveProduct(newProduct));
    }
    [TestMethod]
    public void RemoveNullProductThrows()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);
        Product nullProduct = null;

        Assert.ThrowsException<ArgumentNullException>(() =>
            promotion.RemoveProduct(nullProduct));
    }
    #endregion

    #region UpdateTests

    [TestMethod]
    public void UpdatePromotion_WithValidData_ShouldUpdateFields()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);
        var newStart = DateTime.Now.AddDays(10);
        var newEnd = newStart.AddDays(15);

        promotion.Update("Nueva12345", newStart, newEnd, 50);

        Assert.AreEqual("Nueva12345", promotion.Name);
        Assert.AreEqual(newStart, promotion.StartDate);
        Assert.AreEqual(newEnd, promotion.EndDate);
        Assert.AreEqual(50, promotion.Discount);
    }

    [TestMethod]
    public void UpdatePromotion_WithEmptyName_ShouldThrowException()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        Assert.ThrowsException<ArgumentNullException>(() =>
            promotion.Update("", _startDate, _endDate, _discount)
        );
    }

    [TestMethod]
    public void UpdatePromotion_WithInvalidNameLength_ShouldThrowException()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        Assert.ThrowsException<ArgumentException>(() =>
            promotion.Update("Ab1", _startDate, _endDate, _discount)
        );
    }

    [TestMethod]
    public void UpdatePromotion_WithInvalidEndDate_ShouldThrowException()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        Assert.ThrowsException<ArgumentException>(() =>
            promotion.Update(_name, _startDate, _startDate.AddDays(-1), _discount)
        );
    }

    [TestMethod]
    public void UpdatePromotion_WithInvalidDiscount_ShouldThrowException()
    {
        var promotion = new Promotion(_name, _startDate, _endDate, _discount, _commercialLine, _products);

        Assert.ThrowsException<ArgumentException>(() =>
            promotion.Update(_name, _startDate, _endDate, 0)
        );

        Assert.ThrowsException<ArgumentException>(() =>
            promotion.Update(_name, _startDate, _endDate, 100)
        );
    }

    #endregion
}