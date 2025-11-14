using Cinema.Core.models.contract;
using Cinema.Core.models.customers;
using Cinema.Core.models.roles;
using Cinema.Core.models.sessions;
using Cinema.Core.models;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class OrderTests
    {
        private Customer _testCustomer = null!;
        private Employee _testCashier = null!;
        private Ticket _testTicket = null!;

        private List<Ticket> GetPrivateTickets(Order order)
        {
            return (List<Ticket>)order.GetType()
                .GetProperty("Tickets", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                .GetValue(order)!;
        }

        [SetUp]
        public void Setup()
        {
            // Clear extents
            Order.All.Clear();
            Customer.All.Clear();
            Ticket.All.Clear();
            Session.All.Clear();

            _testCustomer = new Customer("John", "Doe", new DateOnly(1990, 1, 1), "john@example.com", "Password123");

            // Create cashier employee WITH the role
            _testCashier = new Employee("Jane", "Cashier", new DateOnly(1995, 1, 1), new DateOnly(2020, 1, 1), "555-1234",
                new FullTimeContract(2000m, true));
            _testCashier.AddRole(new CashierRole("cashier01", "SecurePass123!"));

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

            // Assert - Use reflection helper for private Tickets property
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
            Assert.That(order.TypeOfOrder, Is.EqualTo(TypeOfOrder.Online));
            Assert.That(GetPrivateTickets(order), Is.EqualTo(tickets));
        }

        [Test]
        public void Constructor_BoxOfficeOrderWithoutCashier_ShouldThrowException()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert - Must pass cashier parameter
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, tickets,
                    customer: null, cashier: null));
            Assert.That(ex.Message, Does.Contain("Box office order must have an associated cashier"));
        }

        [Test]
        public void Constructor_BoxOfficeOrderWithEmployeeWithoutCashierRole_ShouldThrowException()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };
            var regularEmployee = new Employee("Bob", "Worker", new DateOnly(1995, 1, 1),
                new DateOnly(2020, 1, 1), "555-5678", new FullTimeContract(2000m, false));
            // No CashierRole added!

            // Act & Assert - Employee must have CashierRole
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, tickets,
                    customer: null, cashier: regularEmployee));
            Assert.That(ex.Message, Does.Contain("does not have CashierRole"));
        }

        [Test]
        public void Constructor_OnlineOrderWithEmployeeCashier_ShouldAllow()
        {
            // Arrange - Online orders should NOT have cashier
            var tickets = new List<Ticket> { _testTicket };

            // Act & Assert - This should throw because online orders shouldn't have cashiers
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, tickets,
                    customer: _testCustomer, cashier: _testCashier));
            Assert.That(ex.Message, Does.Contain("Online order cannot have a cashier"));
        }

        [Test]
        public void Constructor_BoxOfficeOrderWithValidCashier_ShouldCreateOrder()
        {
            // Arrange
            var tickets = new List<Ticket> { _testTicket };

            // Act
            var order = new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, tickets,
                customer: null, cashier: _testCashier);

            // Assert - Direct property access
            Assert.That(order.Status, Is.EqualTo(OrderStatus.Pending));
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