using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class ProductTests
{
    private string _code = "AXA25";
    private string _name = "NombreValido";
    private string _description = "Descripcion que cumple los criterios";
    private CommercialLine _commercialLine = new CommercialLine("Minutas clásicas");
    private Category _category = new Category("Fritos");
    private List<ProductPicture> _pictures = new List<ProductPicture>()
    {
        new ProductPicture("foto.jpg", 400)
    };
    private decimal _price = 100;

    [TestMethod]
    public void NewProductWithInvalidNameReturnsError()
    {
        string shortName = "Menor";
        string longName = "Mayor|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|";
        string nullName = "";

        Assert.ThrowsException<ArgumentException>(() => new Product(_code, shortName, _description, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentException>(() => new Product(_code, longName, _description, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentNullException>(() => new Product(_code, nullName, _description, _commercialLine, _category, _pictures, _price));
    }

    [TestMethod]
    public void NewProductWithInvalidDescriptionReturnsError()
    {
        string shortDescription = "Menor";
        string longDescription = "Mayor|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|" +
                                 "|++++|++++|++++|++++|++++|++++|++++|++++|++++|++++|";
        string nullDescription = "";

        Assert.ThrowsException<ArgumentException>(() => new Product(_code, _name, shortDescription, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentException>(() => new Product(_code, _name, longDescription, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentNullException>(() => new Product(_code, _name, nullDescription, _commercialLine, _category, _pictures, _price));
    }

    [TestMethod]
    public void NewProductWithInvalidCodeThrowsArgumentException()
    {
        string shortCode = "ABC";
        string longCode = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string nonAlphNumericCode = "AB%34AW";

        Assert.ThrowsException<ArgumentNullException>(() =>
            new Product(null, _name, _description, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentException>(() =>
            new Product(shortCode, _name, _description, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentException>(() =>
            new Product(longCode, _name, _description, _commercialLine, _category, _pictures, _price));
        Assert.ThrowsException<ArgumentException>(() =>
            new Product(nonAlphNumericCode, _name, _description, _commercialLine, _category, _pictures, _price));
    }

    [TestMethod]
    public void NewProductWithNullCommercialLineReturnsError()
    {
        CommercialLine nullCommercialLine = null!;

        Assert.ThrowsException<ArgumentNullException>(() => new Product(_code, _name, _description, nullCommercialLine, _category, _pictures, _price));
    }

    [TestMethod]
    public void NewProductWithNullCategoryReturnsError()
    {
        Category nullCategory = null!;

        Assert.ThrowsException<ArgumentNullException>(() => new Product(_code, _name, _description, _commercialLine, nullCategory, _pictures, _price));
    }

    [TestMethod]
    public void NewProductWithInvalidAmountOfPicturesReturnsError()
    {
        List<ProductPicture> emptyPictures = new List<ProductPicture>();
        List<ProductPicture> tooManyPictures = new List<ProductPicture>()
        {
            new ProductPicture("foto1.jpg", 100),
            new ProductPicture("foto2.jpg", 100),
            new ProductPicture("foto3.jpg", 100),
            new ProductPicture("foto4.jpg", 100)
        };

        Assert.ThrowsException<ArgumentException>(() =>
            new Product(_code, _name, _description, _commercialLine, _category, emptyPictures, _price));
        Assert.ThrowsException<ArgumentException>(() =>
            new Product(_code, _name, _description, _commercialLine, _category, tooManyPictures, _price));
    }

    [TestMethod]
    public void NewProductWithNullPicturesReturnsError()
    {
        List<ProductPicture> nullPictures = null!;

        Assert.ThrowsException<ArgumentNullException>(() =>
            new Product(_code, _name, _description, _commercialLine, _category, nullPictures, _price));
    }

    [TestMethod]
    public void NewProductStoresPriceCorrectly()
    {
        decimal price = 250;

        Product product = new Product(
            _code,
            _name,
            _description,
            _commercialLine,
            _category,
            _pictures,
            price
        );

        Assert.AreEqual(price, product.Price);
    }

    [TestMethod]
    public void NewProductWithInvalidPriceReturnsError()
    {
        Assert.ThrowsException<ArgumentException>(() =>
            new Product(_code, _name, _description, _commercialLine, _category, _pictures, 0));

        Assert.ThrowsException<ArgumentException>(() =>
            new Product(_code, _name, _description, _commercialLine, _category, _pictures, -10));
    }

    [TestMethod]
    public void AddPictureAddsPictureToProduct()
    {
        Product product = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
        ProductPicture newPicture = new ProductPicture("foto2.jpg", 300);

        product.AddPicture(newPicture);

        Assert.AreEqual(2, product.Pictures.Count);
        Assert.IsTrue(product.Pictures.Contains(newPicture));
    }

    [TestMethod]
    public void AddPictureToProductWithThreePicturesReturnsError()
    {
        List<ProductPicture> pictures = new List<ProductPicture>()
        {
            new ProductPicture("foto1.jpg", 100),
            new ProductPicture("foto2.jpg", 100),
            new ProductPicture("foto3.jpg", 100)
        };

        Product product = new Product(_code, _name, _description, _commercialLine, _category, pictures, _price);
        ProductPicture newPicture = new ProductPicture("foto4.jpg", 100);

        Assert.ThrowsException<ArgumentException>(() => product.AddPicture(newPicture));
    }

    [TestMethod]
    public void AddNullPictureReturnsError()
    {
        Product product = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
        ProductPicture nullPicture = null!;

        Assert.ThrowsException<ArgumentNullException>(() => product.AddPicture(nullPicture));
    }

    [TestMethod]
    public void RemovePictureRemovesPictureFromProduct()
    {
        ProductPicture picture1 = new ProductPicture("foto1.jpg", 100);
        ProductPicture picture2 = new ProductPicture("foto2.jpg", 100);

        List<ProductPicture> pictures = new List<ProductPicture>() { picture1, picture2 };

        Product product = new Product(_code, _name, _description, _commercialLine, _category, pictures, _price);

        product.RemovePicture(picture2);

        Assert.AreEqual(1, product.Pictures.Count);
        Assert.IsFalse(product.Pictures.Contains(picture2));
        Assert.IsTrue(product.Pictures.Contains(picture1));
    }

    [TestMethod]
    public void RemoveLastPictureReturnsError()
    {
        Product product = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);

        Assert.ThrowsException<ArgumentException>(() => product.RemovePicture(_pictures[0]));
    }

    [TestMethod]
    public void RemoveNullPictureReturnsError()
    {
        Product product = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
        ProductPicture nullPicture = null!;

        Assert.ThrowsException<ArgumentNullException>(() => product.RemovePicture(nullPicture));
    }

    [TestMethod]
    public void RemovePictureThatDoesNotExistReturnsError()
    {
        Product product = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
        ProductPicture otherPicture = new ProductPicture("otra.jpg", 100);

        Assert.ThrowsException<ArgumentException>(() => product.RemovePicture(otherPicture));
    }

    [TestMethod]
    public void ProductChangeActivationWorks()
    {
        Product productT = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
        Product productF = new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);

        productF.ChangeActivation();

        Assert.IsTrue(productT.IsActive);
        Assert.IsFalse(productF.IsActive);
    }

    [TestMethod]
    public void SetPictures_ReplacesExistingPictures()
    {
        var product = CreateValidProductWithPictures(2);

        var newPictures = new List<ProductPicture>
        {
            new ProductPicture("new1.jpg", 100),
            new ProductPicture("new2.jpg", 100)
        };

        product.SetPictures(newPictures);

        Assert.AreEqual(2, product.Pictures.Count);
        Assert.IsTrue(product.Pictures.All(p => p.FileName.Contains("new")));
    }

    [TestMethod]
    public void SetPictures_Throws_WhenListIsEmpty()
    {
        var product = CreateValidProductWithPictures(2);
        var newPictures = new List<ProductPicture>();

        var ex = Assert.ThrowsException<ArgumentException>(() =>
            product.SetPictures(newPictures)
        );

        StringAssert.Contains(ex.Message, "al menos una imagen");
    }

    [TestMethod]
    public void SetPictures_Throws_WhenMoreThanThreePictures()
    {
        var product = CreateValidProductWithPictures(2);

        var newPictures = new List<ProductPicture>
        {
            new ProductPicture("1.jpg", 100),
            new ProductPicture("2.jpg", 100),
            new ProductPicture("3.jpg", 100),
            new ProductPicture("4.jpg", 100)
        };

        Assert.ThrowsException<ArgumentException>(() =>
            product.SetPictures(newPictures));
    }

    [TestMethod]
    public void SetPictures_Throws_WhenNull()
    {
        var product = CreateValidProductWithPictures(2);

        var ex = Assert.ThrowsException<ArgumentNullException>(() =>
            product.SetPictures(null)
        );

        StringAssert.Contains(ex.Message, "lista de imágenes");
    }

    private Product CreateValidProductWithPictures(int count)
    {
        var pictures = Enumerable.Range(1, count)
            .Select(i => new ProductPicture($"pic{i}.jpg", 100))
            .ToList();

        return new Product(
            _code,
            _name,
            _description,
            _commercialLine,
            _category,
            pictures,
            _price
        );
    }

    private Product CreateValidProduct()
    {
        return new Product(_code, _name, _description, _commercialLine, _category, _pictures, _price);
    }

    #region UpdateTests

    [TestMethod]
    public void UpdateProduct_WithValidData_ShouldUpdateFields()
    {
        var product = CreateValidProduct();
        var newCommercialLine = new CommercialLine("Nueva línea");
        var newCategory = new Category("Nueva categoría");

        product.Update("NuevoNombre", "Nueva descripcion que cumple los criterios", 200, newCommercialLine, newCategory);

        Assert.AreEqual("NuevoNombre", product.Name);
        Assert.AreEqual("Nueva descripcion que cumple los criterios", product.Description);
        Assert.AreEqual(200, product.Price);
        Assert.AreEqual(newCommercialLine, product.CommercialLine);
        Assert.AreEqual(newCategory, product.Category);
    }

    [TestMethod]
    public void UpdateProduct_WithEmptyName_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentNullException>(() =>
            product.Update("", _description, _price, _commercialLine, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithInvalidNameLength_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentException>(() =>
            product.Update("Corto", _description, _price, _commercialLine, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithEmptyDescription_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentNullException>(() =>
            product.Update(_name, "", _price, _commercialLine, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithInvalidDescriptionLength_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentException>(() =>
            product.Update(_name, "Corta", _price, _commercialLine, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithInvalidPrice_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentException>(() =>
            product.Update(_name, _description, 0, _commercialLine, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithNullCommercialLine_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentNullException>(() =>
            product.Update(_name, _description, _price, null!, _category)
        );
    }

    [TestMethod]
    public void UpdateProduct_WithNullCategory_ShouldThrowException()
    {
        var product = CreateValidProduct();

        Assert.ThrowsException<ArgumentNullException>(() =>
            product.Update(_name, _description, _price, _commercialLine, null!)
        );
    }

    #endregion
}
