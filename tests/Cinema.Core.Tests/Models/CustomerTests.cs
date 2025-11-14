using Cinema.Core.models.customers;
using Cinema.Core.models.sessions;
using Cinema.Core.models.sales;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class CustomerTests
    {
        [SetUp]
        public void Setup()
        {
            Customer.All.Clear();
        }

        [Test]
        public void Constructor_WithValidData_ShouldCreateCustomer()
        {
            // Arrange
            var firstName = "Jane";
            var lastName = "Smith";
            var dateOfBirth = new DateOnly(1995, 5, 15);
            var email = "jane.smith@example.com";
            var password = "SecurePass123";

            // Act
            var customer = new Customer(firstName, lastName, dateOfBirth, email, password);

            // Assert
            Assert.That(customer.Email, Is.EqualTo(email));
            Assert.That(customer.HashPassword, Is.Not.Null.And.Not.Empty);
            Assert.That(customer.HashPassword, Is.Not.EqualTo(password)); // Should be hashed
            Assert.That(customer.FirstName, Is.EqualTo(firstName));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_WithInvalidEmail_ShouldThrowException(string invalidEmail)
        {
            // Arrange
            var dateOfBirth = new DateOnly(1995, 5, 15);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", dateOfBirth, invalidEmail, "Password123"));
            Assert.That(ex.Message, Does.Contain("Email cannot be empty").Or.Contain("Invalid email format"));
        }

        [Test]
        public void Constructor_WithNullEmail_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), null!, "Password123"));
            Assert.That(ex.Message, Does.Contain("Email cannot be empty"));
        }

        [Test]
        public void Constructor_WithInvalidEmailFormat_ShouldThrowException()
        {
            // Arrange
            var invalidEmail = "invalid-email";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), invalidEmail, "Password123"));
            Assert.That(ex.Message, Does.Contain("Invalid email format"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_WithEmptyPassword_ShouldThrowException(string invalidPassword)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), "test@example.com", invalidPassword));
            Assert.That(ex.Message, Does.Contain("Password cannot be empty"));
        }

        [Test]
        public void Constructor_WithNullPassword_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), "test@example.com", null!));
            Assert.That(ex.Message, Does.Contain("Password cannot be empty"));
        }

        [Test]
        public void Constructor_WithShortPassword_ShouldThrowException()
        {
            // Arrange
            var shortPassword = "Pass";

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), "test@example.com", shortPassword));
            Assert.That(ex.Message, Does.Contain("Password must be at least 6 characters long"));
        }

        [Test]
        public void StaticExtent_ShouldStoreCustomersCorrectly()
        {
            // Arrange
            var initialCount = Customer.All.Count;

            // Act
            var customer1 = new Customer("John", "Doe", new DateOnly(1990, 1, 1), "john@example.com", "Password123");
            var customer2 = new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), "jane@example.com", "Password123");

            // Assert
            Assert.That(Customer.All.Count, Is.EqualTo(initialCount + 2));
            Assert.That(Customer.All, Contains.Item(customer1));
            Assert.That(Customer.All, Contains.Item(customer2));
        }

        [Test]
        public void AddOrder_ShouldAddOrderToCustomer()
        {
            // Arrange
            var customer = CreateTestCustomer();
            var order = CreateTestOrder();

            // Act
            customer.AddOrder(order);

            // Assert
            Assert.That(customer.Orders, Contains.Item(order));
        }

        [Test]
        public void AddOrder_WithNullOrder_ShouldThrowException()
        {
            // Arrange
            var customer = CreateTestCustomer();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => customer.AddOrder(null!));
            Assert.That(ex.ParamName, Is.EqualTo("order"));
        }

        [Test]
        public void HashPassword_ShouldBeConsistent()
        {
            // Arrange
            var password = "SecurePass123";
            var customer1 = CreateTestCustomer(password: password);
            var customer2 = CreateTestCustomer(password: password);

            // Act
            var hash1 = customer1.HashPassword;
            var hash2 = customer2.HashPassword;

            // Assert
            Assert.That(hash1, Is.EqualTo(hash2));
            Assert.That(hash1, Is.Not.EqualTo(password));
        }

        private Customer CreateTestCustomer(string email = "test@example.com", string password = "Password123")
        {
            return new Customer("Test", "User", new DateOnly(1995, 1, 1), email, password);
        }

        private Order CreateTestOrder()
        {
            var hall = new Hall("Test Hall", 100);
            var movie = new Movie("Test Movie", TimeSpan.FromHours(2), new[] { "Action" });
            var session = new Session(hall, movie, DateTime.Now.AddDays(1), "English");
            var seat = new Seat(SeatType.Normal, 10.00m, false);
            var ticket = new Ticket(session, seat);

            Order.All.Clear();

            return new Order(
                DateTime.Now,
                TypeOfOrder.Online,
                OrderStatus.Pending,
                new List<Ticket> { ticket },
                CreateTestCustomer()
            );
        }
    }
}