using BusinessLogic;
using Domain;
using IRepository;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Linq.Expressions;
using IBussinesLogic;

namespace BussinesLogicTests;

[TestClass]
public class ReportServiceTests
{
    private Mock<IOrderRepository> _orderRepositoryMock = null!;
    private Mock<IGenericRepository<Product>> _productRepositoryMock = null!;
    private IReportService _reportService = null!;

    private User CreateValidUser(Guid id)
    {
        User user = new User(
            "Juan",
            "Perez",
            "juan@test.com",
            "099123456",
            "Password123ssssss!",
            Domain.Enums.Role.Client
        );

        user.Id = id;
        return user;
    }

    private Order CreateValidOrder(Guid customerId, User customer, DateTime createdAt)
    {
        Order order = new Order(
            customerId,
            new ShippingType("hola", 10),
            new DeliveryAddress("Rivera", "1234", "101")
        );

        order.GetType()
            .GetProperty("Customer")!
            .SetValue(order, customer);

        order.GetType()
            .GetProperty("CreatedAt")!
            .SetValue(order, createdAt);

        return order;
    }

    private void AddLineWithProduct(Order order, Product product, int quantity)
    {
        var line = new OrderLine(product.Id, quantity, product.Price, 0);

        line.GetType()
            .GetProperty("Product")!
            .SetValue(line, product);

        order.AddLine(line);
    }

    private Product CreateValidProduct(Guid productId)
    {
        CommercialLine commercialLine = new CommercialLine("Minutas");
        Category category = new Category("Fritos");

        List<ProductPicture> pictures = new List<ProductPicture>
        {
            new ProductPicture("milanesa.jpg", 100)
        };

        Product product = new Product(
            "PROD01",
            "Milanesa completa",
            "Milanesa completa con papas fritas",
            commercialLine,
            category,
            pictures,
            100
        );

        product.Id = productId;

        return product;
    }

    [TestInitialize]
    public void Setup()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>(MockBehavior.Strict);
        _productRepositoryMock = new Mock<IGenericRepository<Product>>(MockBehavior.Strict);

        _reportService = new ReportService(
            _orderRepositoryMock.Object,
            _productRepositoryMock.Object
        );
    }

    [TestMethod]
    public void GetTopProducts_WhenThereAreNoOrders_ShouldReturnEmptyList()
    {
        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order>());

        var result = _reportService.GetTopProducts(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetTopProducts_WhenThereIsOneOrderLine_ShouldReturnThatProduct()
    {
        Guid productId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Product product = CreateValidProduct(productId);

        Order order = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 15));
        AddLineWithProduct(order, product, 3);

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { order });

        _productRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns(product);

        var result = _reportService.GetTopProducts(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(product.Code, result[0].Code);
        Assert.AreEqual(product.Name, result[0].Name);
        Assert.AreEqual(3, result[0].Quantity);
        Assert.AreEqual(1, result[0].Images.Count);
        Assert.AreEqual("milanesa.jpg", result[0].Images[0]);
    }

    [TestMethod]
    public void GetTopProducts_WhenSameProductAppearsMultipleTimes_ShouldSumQuantities()
    {
        Guid productId = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Product product = CreateValidProduct(productId);

        Order order = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 15));

        AddLineWithProduct(order, product, 2);
        AddLineWithProduct(order, product, 3);

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { order });

        var result = _reportService.GetTopProducts(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(5, result[0].Quantity);
    }

    [TestMethod]
    public void GetTopProducts_ShouldReturnProductsOrderedByQuantityDescending()
    {
        Guid product1Id = Guid.NewGuid();
        Guid product2Id = Guid.NewGuid();
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Product product1 = CreateValidProduct(product1Id);
        product1.Code = "PROD01";

        Product product2 = CreateValidProduct(product2Id);
        product2.Code = "PROD02";

        Order order = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 15));
        AddLineWithProduct(order, product1, 2);
        AddLineWithProduct(order, product2, 5);

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { order });

        _productRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Expression<Func<Product, bool>> predicate) =>
            {
                if (predicate.Compile()(product1)) return product1;
                if (predicate.Compile()(product2)) return product2;
                return null;
            });

        var result = _reportService.GetTopProducts(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("PROD02", result[0].Code);
        Assert.AreEqual(5, result[0].Quantity);
        Assert.AreEqual("PROD01", result[1].Code);
        Assert.AreEqual(2, result[1].Quantity);
    }

    [TestMethod]
    public void GetTopProducts_WhenMoreThanFiveProducts_ShouldReturnOnlyTopFive()
    {
        var orders = new List<Order>();
        var products = new List<Product>();

        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Order order = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 15));

        for (int i = 0; i < 6; i++)
        {
            Guid productId = Guid.NewGuid();

            Product product = CreateValidProduct(productId);
            product.Code = $"PROD{i}";
            products.Add(product);

            AddLineWithProduct(order, product, i + 1);
        }

        orders.Add(order);

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(orders);

        _productRepositoryMock
            .Setup(r => r.GetBy(It.IsAny<Expression<Func<Product, bool>>>()))
            .Returns((Expression<Func<Product, bool>> predicate) =>
            {
                return products.FirstOrDefault(p => predicate.Compile()(p));
            });

        var result = _reportService.GetTopProducts(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(5, result.Count);
    }

    [TestMethod]
    public void GetSalesReport_WhenThereAreNoOrders_ShouldReturnEmptyList()
    {
        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order>());

        var result = _reportService.GetSalesReport(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(0, result.Count);
    }

    [TestMethod]
    public void GetSalesReport_WithOneOrder_ShouldReturnOneMonthWithOneCustomer()
    {
        Guid customerId = Guid.NewGuid();
        User user = CreateValidUser(customerId);

        Order order = CreateValidOrder(customerId, user, new DateTime(2026, 1, 15));
        order.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));
        order.CalculateTotals();

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { order });

        var result = _reportService.GetSalesReport(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        Assert.AreEqual(1, result.Count);

        var month = result[0];

        Assert.AreEqual("2026-01", month.Period);
        Assert.AreEqual(1, month.Customers.Count);

        var customer = month.Customers[0];

        Assert.AreEqual(user.Name + " " + user.Surname, customer.CustomerName);
        Assert.AreEqual(order.Total, customer.Total);
    }

    [TestMethod]
    public void GetSalesReport_ShouldReturnMonthsOrderedAscending()
    {
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Order januaryOrder = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 10));
        januaryOrder.AddLine(new OrderLine(Guid.NewGuid(), 1, 100, 0));
        januaryOrder.CalculateTotals();

        Order februaryOrder = CreateValidOrder(customerId, customer, new DateTime(2026, 2, 10));
        februaryOrder.AddLine(new OrderLine(Guid.NewGuid(), 1, 100, 0));
        februaryOrder.CalculateTotals();

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { februaryOrder, januaryOrder });

        var result = _reportService.GetSalesReport(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 2, 28)
        );

        Assert.AreEqual("2026-01", result[0].Period);
        Assert.AreEqual("2026-02", result[1].Period);
    }

    [TestMethod]
    public void GetSalesReport_WithOrdersInDifferentMonths_ShouldReturnMultiplePeriods()
    {
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Order januaryOrder = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 10));
        januaryOrder.AddLine(new OrderLine(Guid.NewGuid(), 1, 100, 0));
        januaryOrder.CalculateTotals();

        Order februaryOrder = CreateValidOrder(customerId, customer, new DateTime(2026, 2, 10));
        februaryOrder.AddLine(new OrderLine(Guid.NewGuid(), 1, 100, 0));
        februaryOrder.CalculateTotals();

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { januaryOrder, februaryOrder });

        var result = _reportService.GetSalesReport(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 2, 28)
        );

        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("2026-01", result[0].Period);
        Assert.AreEqual("2026-02", result[1].Period);
    }

    [TestMethod]
    public void GetSalesReport_ShouldCalculateMonthlyTotalCorrectly()
    {
        Guid customerId = Guid.NewGuid();
        User customer = CreateValidUser(customerId);

        Order order1 = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 10));
        order1.AddLine(new OrderLine(Guid.NewGuid(), 1, 100, 0));
        order1.CalculateTotals();

        Order order2 = CreateValidOrder(customerId, customer, new DateTime(2026, 1, 15));
        order2.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));
        order2.CalculateTotals();

        _orderRepositoryMock
            .Setup(r => r.GetAllUnpagedBy(It.IsAny<Expression<Func<Order, bool>>>()))
            .Returns(new List<Order> { order1, order2 });

        var result = _reportService.GetSalesReport(
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31)
        );

        var month = result[0];

        Assert.AreEqual(order1.Total + order2.Total, month.Total);
    }
}