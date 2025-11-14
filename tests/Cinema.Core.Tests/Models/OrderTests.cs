using Cinema.Core.models.customers;
using Cinema.Core.models.roles;
using Cinema.Core.models.sessions;
using Cinema.Core.models.sales;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class OrderTests
    {
        private Customer _testCustomer = null!;
        private CashierRole _testCashier = null!;
        private Ticket _testTicket = null!;

        [SetUp]
        public void Setup()
        {
            _testCustomer = new Customer("John", "Doe", new DateOnly(1990, 1, 1), "john@example.com", "Password123");
            _testCashier = new CashierRole("cashier01", "SecurePass123!");

            var hall = new Hall("Test Hall", 100);
            var movie = new Movie("Test Movie", TimeSpan.FromHours(2), new[] { "Action" });
            var session = new Session(hall, movie, DateTime.Now.AddDays(1), "English");
            _testTicket = new Ticket(session, new Seat(SeatType.Normal, 10.00m, false));
        }

        [Test]
        public void Constructor_WithValidOnlineOrder_ShouldCreateOrder()
        {
            // Arrange
            var createdAt = DateTime.Now;
            var tickets = new List<Ticket> { _testTicket };

            // Act
            var order = new Order(createdAt, TypeOfOrder.Online, OrderStatus.Pending, tickets, _testCustomer);

            // Assert - Use reflection to access private properties for testing
            var status = GetPrivateProperty(order, "Status");
            var orderType = GetPrivateProperty(order, "TypeOfOrder");
            var ticketsList = GetPrivateProperty(order, "Tickets");

            Assert.That(status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(orderType, Is.EqualTo(TypeOfOrder.Online));
            Assert.That(ticketsList, Is.EqualTo(tickets));
        }

        [Test]
        public void Constructor_WithFutureCreatedAt_ShouldThrowException()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(1);
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(futureDate, TypeOfOrder.Online, OrderStatus.Pending, tickets, _testCustomer));
            Assert.That(ex.Message, Does.Contain("CreatedAt cannot be in the future"));
        }

        [Test]
        public void Constructor_WithEmptyTickets_ShouldThrowException()
        {
            // Arrange
            var emptyTickets = new List<Ticket>();

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, emptyTickets, _testCustomer));
            Assert.That(ex.Message, Does.Contain("Order must contain at least one ticket"));
        }

        [Test]
        public void Constructor_WithNullTickets_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, null!, _testCustomer));
            Assert.That(ex.Message, Does.Contain("Order must contain at least one ticket"));
        }

        [Test]
        public void Constructor_OnlineOrderWithoutCustomer_ShouldThrowException()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, tickets, null));
            Assert.That(ex.Message, Does.Contain("Online order must have an associated customer"));
        }

        [Test]
        public void Constructor_BoxOfficeOrderWithoutCashier_ShouldThrowException()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, tickets,
                    cashier: null, customer: null));
            Assert.That(ex.Message, Does.Contain("Box office order must have an associated cashier"));
        }

        [Test]
        public void Constructor_OnlineOrderWithCashier_ShouldThrowException()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, tickets,
                    customer: _testCustomer, cashier: _testCashier));
            Assert.That(ex.Message, Does.Contain("Online order cannot have a cashier"));
        }

        [Test]
        public void FinalizeOrder_ShouldChangeStatusToPaid()
        {
            // Arrange
            var order = CreateTestOrder();

            // Act
            order.FinalizeOrder();

            // Assert
            var status = GetPrivateProperty(order, "Status");
            Assert.That(status, Is.EqualTo(OrderStatus.Paid));
        }

        [Test]
        public void FinalizeOrder_WhenAlreadyPaid_ShouldThrowException()
        {
            // Arrange
            var order = CreateTestOrder();
            order.FinalizeOrder();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => order.FinalizeOrder());
            Assert.That(ex.Message, Does.Contain("Cannot finalize order"));
        }

        [Test]
        public void RequestRefund_ShouldChangeStatusToRefunded()
        {
            // Arrange
            var order = CreateTestOrder();
            order.FinalizeOrder();

            // Act
            order.RequestRefund();

            // Assert
            var status = GetPrivateProperty(order, "Status");
            Assert.That(status, Is.EqualTo(OrderStatus.Refunded));
        }

        [Test]
        public void RequestRefund_WhenNotPaid_ShouldThrowException()
        {
            // Arrange
            var order = CreateTestOrder(); // Status is Pending

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => order.RequestRefund());
            Assert.That(ex.Message, Does.Contain("Cannot refund order"));
        }

        private Order CreateTestOrder()
        {
            var tickets = new List<Ticket> { _testTicket };
            return new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, tickets, _testCustomer);
        }

        private object GetPrivateProperty(object obj, string propertyName)
        {
            return obj.GetType()
                .GetProperty(propertyName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(obj)!;
        }
    }
}