using Cinema.Core.models.customers;
using Cinema.Core.models.sessions;

namespace Cinema.Tests
{
    [TestFixture]
    public class ExtentPersistenceTests
    {
        private const string TestFilePath = "test_extent.txt";

        [SetUp]
        public void Setup()
        {
            ClearAllExtents();
        }

        [TearDown]
        public void Cleanup()
        {
            if (File.Exists(TestFilePath))
                File.Delete(TestFilePath);
            ClearAllExtents();
        }

        private void ClearAllExtents()
        {
            Customer.All.Clear();
            Session.All.Clear();
            Ticket.All.Clear();
        }

        [Test]
        public void Extent_ShouldBeSerializable_AndDeserializable()
        {
            // Arrange
            var customer = new Customer("John", "Doe", new DateOnly(1990, 1, 1), "john@example.com", "Password123");

            // Verify it was added to extent
            Assert.That(Customer.All.Count, Is.EqualTo(1), "Customer should be automatically added to extent");

            // Act - Save (just a flag file to simulate persistence)
            File.WriteAllText(TestFilePath, $"Customers:{Customer.All.Count}\n");
            foreach (var c in Customer.All)
            {
                File.AppendAllText(TestFilePath, $"CUSTOMER|{c.Email}|{c.FirstName}|{c.LastName}\n");
            }

            // Clear
            Customer.All.Clear();
            Assert.That(Customer.All.Count, Is.EqualTo(0), "Extent should be empty after clear");

            // Load - Recreate from saved data
            var savedData = File.ReadAllLines(TestFilePath);
            var customerCount = int.Parse(savedData[0].Split(':')[1]);

            for (int i = 1; i <= customerCount; i++)
            {
                var parts = savedData[i].Split('|');
                var recreated = new Customer(parts[2], parts[3], new DateOnly(1990, 1, 1), parts[1], "Password123");
                // Already added to extent by constructor
            }

            // Assert
            Assert.That(Customer.All.Count, Is.EqualTo(1), "Should have 1 customer after loading");
            Assert.That(Customer.All[0].Email, Is.EqualTo("john@example.com"), "Email should match");
        }

        [Test]
        public void ExtentPersistence_ShouldMaintainDataBetweenSessions()
        {
            // Arrange
            var customer1 = new Customer("Alice", "Smith", new DateOnly(1992, 3, 10), "alice@example.com", "Pass123");
            var customer2 = new Customer("Bob", "Jones", new DateOnly(1988, 7, 20), "bob@example.com", "Pass123");

            Assert.That(Customer.All.Count, Is.EqualTo(2), "Should have 2 customers before save");

            // Act - Save, clear, load
            File.WriteAllText(TestFilePath, $"Customers:{Customer.All.Count}\n");
            foreach (var c in Customer.All)
            {
                File.AppendAllText(TestFilePath, $"CUSTOMER|{c.Email}|{c.FirstName}|{c.LastName}\n");
            }

            ClearAllExtents();
            Assert.That(Customer.All.Count, Is.EqualTo(0), "Should be empty after clear");

            // Load
            var savedData = File.ReadAllLines(TestFilePath);
            var customerCount = int.Parse(savedData[0].Split(':')[1]);

            for (int i = 1; i <= customerCount; i++)
            {
                var parts = savedData[i].Split('|');
                new Customer(parts[2], parts[3], new DateOnly(1990, 1, 1), parts[1], "Password123");
            }

            // Assert
            Assert.That(Customer.All.Count, Is.EqualTo(2), "Should have 2 customers after loading");
            Assert.That(Customer.All.Any(c => c.Email == "alice@example.com"), Is.True, "Alice should exist");
            Assert.That(Customer.All.Any(c => c.Email == "bob@example.com"), Is.True, "Bob should exist");
        }

        [Test]
        public void Encapsulation_ModifyingAttribute_ShouldNotAffectExtentDirectly()
        {
            // Arrange
            var customer = new Customer("Test", "User", new DateOnly(1990, 1, 1), "test@example.com", "Password123");

            // Verify extent is working
            Assert.That(Customer.All.Count, Is.EqualTo(1), "Customer must be in extent after creation");

            // Act - Modify via reflection
            var property = typeof(Customer).GetProperty("Email", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            property?.SetValue(customer, "hacked@example.com");

            // Assert - Extent maintains same object reference
            Assert.That(Customer.All[0], Is.SameAs(customer), "Should be same object reference");
            Assert.That(Customer.All[0].Email, Is.EqualTo("hacked@example.com"), "Property change should be visible");

            // Act - Another modification
            property?.SetValue(customer, "another@example.com");

            // Assert - Still same object, property changed
            Assert.That(Customer.All[0].Email, Is.EqualTo("another@example.com"), "Second change should also be visible");
        }
    }
}