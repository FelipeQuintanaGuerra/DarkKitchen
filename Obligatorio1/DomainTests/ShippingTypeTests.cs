using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class ShippingTypeTests
{
    [TestMethod]
    public void NewShippingType_ShouldStoreNameAndCost()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        Assert.AreEqual("Envío express", shippingType.Name);
        Assert.AreEqual(250, shippingType.Cost);
    }

    [TestMethod]
    public void NewShippingType_ShouldGenerateId()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        Assert.AreNotEqual(Guid.Empty, shippingType.Id);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewShippingType_WithNullOrWhiteSpaceName_ShouldThrowException()
    {
        new ShippingType("", 250);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewShippingType_WithNegativeCost_ShouldThrowException()
    {
        new ShippingType("Envío express", -1);
    }

    [TestMethod]
    public void NewShippingType_WithZeroCost_ShouldBeValid()
    {
        ShippingType shippingType = new ShippingType("Envío gratis", 0);

        Assert.AreEqual(0, shippingType.Cost);
    }

    [TestMethod]
    public void Update_ShouldModifyNameAndCost()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        shippingType.Update("Envío premium", 400);

        Assert.AreEqual("Envío premium", shippingType.Name);
        Assert.AreEqual(400, shippingType.Cost);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Update_WithEmptyName_ShouldThrowException()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        shippingType.Update("", 400);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void Update_WithNegativeCost_ShouldThrowException()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        shippingType.Update("Envío premium", -1);
    }

    [TestMethod]
    public void Update_WithZeroCost_ShouldBeValid()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        shippingType.Update("Envío gratis", 0);

        Assert.AreEqual("Envío gratis", shippingType.Name);
        Assert.AreEqual(0, shippingType.Cost);
    }

    [TestMethod]
    public void NewShippingType_IsDefault_ShouldBeFalseByDefault()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        Assert.IsFalse(shippingType.IsDefault);
    }
}