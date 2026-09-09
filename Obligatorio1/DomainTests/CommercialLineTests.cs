using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class CommercialLineTests
{
    [TestMethod]
    public void NewClWithNullNameThrowsArgumentNullException()
    { // Act + Assert
        Assert.ThrowsException<ArgumentNullException>(() => new CommercialLine(null));
    }
    [TestMethod]
    public void NewClWithEmptyNameThrowsArgumentException()
    { // Act + Assert
        Assert.ThrowsException<ArgumentException>(() => new CommercialLine(""));
    }
}