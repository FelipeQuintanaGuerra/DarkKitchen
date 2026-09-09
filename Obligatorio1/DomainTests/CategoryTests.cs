using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class CategoryTests
{
    [TestMethod]
    public void NewCategoryWithNullNameThrowsArgumentNullException()
    { // Act + Assert
        Assert.ThrowsException<ArgumentNullException>(() => new Category(null));
    }
}