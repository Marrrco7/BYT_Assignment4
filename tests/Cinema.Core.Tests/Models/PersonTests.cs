using Cinema.Core.models.customers;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class PersonTests
    {
        [Test]
        public void Constructor_WithValidData_ShouldCreatePerson()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var dateOfBirth = new DateOnly(1990, 1, 1);

            // Act
            var person = new TestPerson(firstName, lastName, dateOfBirth);

            // Assert
            Assert.That(person.FirstName, Is.EqualTo(firstName));
            Assert.That(person.LastName, Is.EqualTo(lastName));
            Assert.That(person.Age, Is.GreaterThan(0));
        }

        [Test]
        public void Constructor_WithEmptyFirstName_ShouldThrowException()
        {
            // Arrange
            var lastName = "Doe";
            var dateOfBirth = new DateOnly(1990, 1, 1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new TestPerson("", lastName, dateOfBirth));
            Assert.That(ex.Message, Does.Contain("First name cannot be null, empty or whitespace."));
        }

        [Test]
        public void Constructor_WithEmptyLastName_ShouldThrowException()
        {
            // Arrange
            var firstName = "John";
            var dateOfBirth = new DateOnly(1990, 1, 1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new TestPerson(firstName, "", dateOfBirth));
            Assert.That(ex.Message, Does.Contain("Last name cannot be null, empty or whitespace."));
        }

        [Test]
        public void Constructor_WithFutureDateOfBirth_ShouldThrowException()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var futureDate = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new TestPerson(firstName, lastName, futureDate));
            Assert.That(ex.Message, Does.Contain("Date of birth cannot be in the future."));
        }

        [Test]
        public void Constructor_WithDateOfBirthTooOld_ShouldThrowException()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var ancientDate = DateOnly.FromDateTime(DateTime.Now).AddYears(-121);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() => new TestPerson(firstName, lastName, ancientDate));
            Assert.That(ex.Message, Does.Contain("Age must be between 0 and 120 years."));
        }

        [Test]
        public void Age_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var birthYear = DateTime.Now.Year - 25;
            var person = new TestPerson("John", "Doe", new DateOnly(birthYear, 1, 1));

            // Act
            var age = person.Age;

            // Assert
            Assert.That(age, Is.EqualTo(25));
        }

        // Test subclass to test abstract Person class
        private class TestPerson : Person
        {
            public TestPerson(string firstName, string lastName, DateOnly dateOfBirth)
                : base(firstName, lastName, dateOfBirth) { }
        }
    }
}