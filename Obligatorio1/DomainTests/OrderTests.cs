using Domain;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DomainTests;

[TestClass]
public class OrderTests
{
    private Order CreateValidOrder()
    {
        ShippingType shippingType = new ShippingType("Tipo de envío válido", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");

        return new Order(Guid.NewGuid(), shippingType, address);
    }

    [TestMethod]
    public void NewOrder_ShouldBePending()
    {
        Order order = CreateValidOrder();
        Assert.AreEqual(OrderStatus.Pending, order.Status);
    }

    [TestMethod]
    public void Prepare_PendingOrder_ShouldChangeStatusToPrepared()
    {
        Order order = CreateValidOrder();
        order.Prepare();

        Assert.AreEqual(OrderStatus.Prepared, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Prepare_NonPendingOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.Prepare();
    }

    [TestMethod]
    public void Cancel_PendingOrder_ShouldChangeStatusToCancelled()
    {
        Order order = CreateValidOrder();
        order.Cancel();

        Assert.AreEqual(OrderStatus.Cancelled, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Cancel_NonPendingOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.Cancel();
    }

    [TestMethod]
    public void MarkAsInTransit_PreparedOrder_ShouldChangeStatusToInTransit()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();

        Assert.AreEqual(OrderStatus.InTransit, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void MarkAsInTransit_NonPreparedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.MarkAsInTransit();
    }

    [TestMethod]
    public void Deliver_InTransitOrder_ShouldChangeStatusToDelivered()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();
        order.Deliver();

        Assert.AreEqual(OrderStatus.Delivered, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Deliver_NonInTransitOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Deliver();
    }

    [TestMethod]
    public void MarkAsNotDelivered_InTransitOrder_ShouldChangeStatusToNotDelivered()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();
        order.MarkAsNotDelivered();

        Assert.AreEqual(OrderStatus.NotDelivered, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void MarkAsNotDelivered_NonInTransitOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.MarkAsNotDelivered();
    }

    [TestMethod]
    public void NewOrder_ShouldStoreCustomerId()
    {
        Guid customerId = Guid.NewGuid();
        ShippingType shippingType = new ShippingType("Envío test", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");

        Order order = new Order(customerId, shippingType, address);

        Assert.AreEqual(customerId, order.CustomerId);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void NewOrder_WithInvalidCustomerId_ShouldThrowException()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);

        Order order = new Order(Guid.Empty, shippingType, new DeliveryAddress("Rivera", "1234", "101"));
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void NewOrder_WithoutShippingType_ShouldThrowException()
    {
        Order order = new Order(Guid.NewGuid(), null, new DeliveryAddress("Rivera", "1234", "101"));
    }

    [TestMethod]
    public void NewOrder_ShouldStoreShippingType()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);

        Order order = new Order(Guid.NewGuid(), shippingType, new DeliveryAddress("Rivera", "1234", "101"));

        Assert.AreEqual(shippingType, order.ShippingType);
    }

    [TestMethod]
    public void NewOrder_ShouldStoreDeliveryAddress()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");

        Order order = new Order(Guid.NewGuid(), shippingType, address);

        Assert.AreEqual(address, order.DeliveryAddress);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void NewOrder_WithoutDeliveryAddress_ShouldThrowException()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);

        Order order = new Order(Guid.NewGuid(), shippingType, null);
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void DeliveryAddress_WithEmptyStreet_ShouldThrowException()
    {
        new DeliveryAddress("", "1234", "101");
    }

    [TestMethod]
    [ExpectedException(typeof(Exception))]
    public void DeliveryAddress_WithEmptyDoorNumber_ShouldThrowException()
    {
        new DeliveryAddress("Rivera", "", "101");
    }

    [TestMethod]
    public void DeliveryAddress_WithEmptyApartment_ShouldBeValid()
    {
        var address = new DeliveryAddress("Rivera", "1234", "");

        Assert.AreEqual("", address.Apartment);
    }

    [TestMethod]
    public void OrderLine_ShouldGenerateOwnId()
    {
        OrderLine line = new OrderLine(Guid.NewGuid(), 2, 100, 0);

        Assert.AreNotEqual(Guid.Empty, line.Id);
    }

    [TestMethod]
    public void NewOrder_ShouldStartWithZeroLines()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");

        Order order = new Order(Guid.NewGuid(), shippingType, address);

        Assert.AreEqual(0, order.Lines.Count);
    }

    [TestMethod]
    public void AddLine_ShouldAddLineToOrder()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");
        Order order = new Order(Guid.NewGuid(), shippingType, address);
        OrderLine line = new OrderLine(Guid.NewGuid(), 10, 20, 0.5m);

        order.AddLine(line);

        Assert.AreEqual(1, order.Lines.Count);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void AddLine_NullLine_ShouldThrowException()
    {
        ShippingType shippingType = new ShippingType("Envío test", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");
        Order order = new Order(Guid.NewGuid(), shippingType, address);

        order.AddLine(null);
    }

    [TestMethod]
    public void OrderLine_ShouldStoreValuesCorrectly()
    {
        Guid guid = Guid.NewGuid();
        OrderLine line = new OrderLine(guid, 2, 100, 0);

        Assert.AreEqual(guid, line.ProductId);
        Assert.AreEqual(2, line.Quantity);
        Assert.AreEqual(100, line.UnitPrice);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OrderLine_WithInvalidQuantity_ShouldThrowException()
    {
        new OrderLine(Guid.NewGuid(), 0, 100, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OrderLine_WithInvalidUnitPrice_ShouldThrowException()
    {
        new OrderLine(Guid.NewGuid(), 2, 0, 0);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OrderLine_WithNegativeDiscount_ShouldThrowException()
    {
        new OrderLine(Guid.NewGuid(), 2, 100, -0.1m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentException))]
    public void OrderLine_WithDiscountGreaterThanOne_ShouldThrowException()
    {
        new OrderLine(Guid.NewGuid(), 2, 100, 1.5m);
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void OrderLine_WithInvalidProductId_ShouldThrowException()
    {
        new OrderLine(Guid.Empty, 2, 100, 0);
    }

    [TestMethod]
    public void OrderLine_Subtotal_ShouldBeCalculatedCorrectly()
    {
        OrderLine line = new OrderLine(Guid.NewGuid(), 2, 100, 0.1m);

        Assert.AreEqual(180, line.Subtotal);
    }

    [TestMethod]
    public void CalculateTotals_WithLines_ShouldCalculateSubtotalCorrectly()
    {
        Order order = CreateValidOrder();

        order.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));
        order.AddLine(new OrderLine(Guid.NewGuid(), 1, 300, 0));

        order.CalculateTotals();

        Assert.AreEqual(500, order.Subtotal);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CalculateTotals_WithoutLines_ShouldThrowException()
    {
        Order order = CreateValidOrder();

        order.CalculateTotals();
    }

    [TestMethod]
    public void CalculateTotals_ShouldCalculateTotalWithShippingAndIVA()
    {
        Order order = CreateValidOrder();

        order.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));
        order.AddLine(new OrderLine(Guid.NewGuid(), 1, 300, 0));

        order.CalculateTotals();

        Assert.AreEqual(915, order.Total);
    }

    [TestMethod]
    public void CalculateTotals_ShouldStoreShippingCost()
    {
        Order order = CreateValidOrder();

        order.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));
        order.AddLine(new OrderLine(Guid.NewGuid(), 1, 300, 0));

        order.CalculateTotals();

        Assert.AreEqual(250, order.ShippingCost);
    }

    [TestMethod]
    public void NewOrder_ShouldGenerateOrderNumber()
    {
        Order order = CreateValidOrder();

        Assert.AreNotEqual(Guid.Empty, order.OrderNumber);
    }

    [TestMethod]
    public void NewOrder_ShouldStoreCreatedAt()
    {
        Order order = CreateValidOrder();

        Assert.AreNotEqual(default(DateTime), order.CreatedAt);
    }

    [TestMethod]
    public void NewOrder_ShouldStoreConcreteShippingType()
    {
        ShippingType shippingType = new ShippingType("Envío express", 250);
        DeliveryAddress address = new DeliveryAddress("Rivera", "1234", "101");

        Order order = new Order(Guid.NewGuid(), shippingType, address);

        Assert.AreEqual(shippingType, order.ShippingType);
    }

    [TestMethod]
    public void CalculateTotals_ShouldUseShippingTypeCost()
    {
        ShippingType shippingType = new ShippingType("Express", 250);

        Order order = new Order(
            Guid.NewGuid(),
            shippingType,
            new DeliveryAddress("Rivera", "1234", "101")
        );

        order.AddLine(new OrderLine(Guid.NewGuid(), 2, 100, 0));

        order.CalculateTotals();

        Assert.AreEqual(250, order.ShippingCost);
    }

    [TestMethod]
    public void NewOrder_ShouldStoreShippingTypeId()
    {
        ShippingType shippingType = new ShippingType("Tipo de envío válido", 250);

        Order order = new Order(
            Guid.NewGuid(),
            shippingType,
            new DeliveryAddress("Rivera", "1234", "101")
        );

        Assert.AreEqual(shippingType.Id, order.ShippingTypeId);
    }

    [TestMethod]
    public void Delay_PendingOrder_ShouldChangeStatusToDelayed()
    {
        Order order = CreateValidOrder();
        order.Delay();

        Assert.AreEqual(OrderStatus.Delayed, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_PreparedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.Delay();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_CancelledOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Cancel();
        order.Delay();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_InTransitOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();
        order.Delay();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_DeliveredOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();
        order.Deliver();
        order.Delay();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_NotDeliveredOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Prepare();
        order.MarkAsInTransit();
        order.MarkAsNotDelivered();
        order.Delay();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Delay_DelayedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.Delay();
    }

    [TestMethod]
    public void Prepare_DelayedOrder_ShouldChangeStatusToPrepared()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.Prepare();

        Assert.AreEqual(OrderStatus.Prepared, order.Status);
    }

    [TestMethod]
    public void Cancel_DelayedOrder_ShouldChangeStatusToCancelled()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.Cancel();

        Assert.AreEqual(OrderStatus.Cancelled, order.Status);
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void MarkAsInTransit_DelayedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.MarkAsInTransit();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void Deliver_DelayedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.Deliver();
    }

    [TestMethod]
    [ExpectedException(typeof(ConflictException))]
    public void MarkAsNotDelivered_DelayedOrder_ShouldThrowException()
    {
        Order order = CreateValidOrder();
        order.Delay();
        order.MarkAsNotDelivered();
    }
}