using Cinema.Core.models.customers;
using Cinema.Core.models.contract;
using Cinema.Core.models.roles;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class EmployeeTests
    {
        private EmploymentContract _testContract = null!;

        [SetUp]
        public void Setup()
        {
            _testContract = new FullTimeContract(2000m, true);
        }

        [Test]
        public void Constructor_WithValidData_ShouldCreateEmployee()
        {
            // Arrange
            var firstName = "John";
            var lastName = "Doe";
            var dateOfBirth = new DateOnly(1990, 1, 1);
            var hiringDate = new DateOnly(2020, 6, 1);
            var phoneNumber = "123-456-7890";

            // Act
            var employee = new Employee(firstName, lastName, dateOfBirth, hiringDate, phoneNumber, _testContract);

            // Assert
            Assert.That(employee.FirstName, Is.EqualTo(firstName));
            Assert.That(employee.HiringDate, Is.EqualTo(hiringDate));
            Assert.That(employee.PhoneNumber, Is.EqualTo(phoneNumber));
            Assert.That(employee.Contract, Is.EqualTo(_testContract));
            Assert.That(employee.Roles, Is.Empty);
            Assert.That(employee.Subordinates, Is.Empty);
        }

        [Test]
        public void Constructor_WithFutureHiringDate_ShouldThrowException()
        {
            // Arrange
            var futureDate = DateOnly.FromDateTime(DateTime.Now).AddDays(1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Employee("John", "Doe", new DateOnly(1990, 1, 1), futureDate, "123-456-7890", _testContract));
            Assert.That(ex.Message, Does.Contain("Hiring date cannot be in the future"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_WithEmptyPhoneNumber_ShouldThrowException(string invalidPhone)
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Employee("John", "Doe", new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), invalidPhone, _testContract));
            Assert.That(ex.Message, Does.Contain("Phone number cannot be empty"));
        }

        [Test]
        public void Constructor_WithNullPhoneNumber_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Employee("John", "Doe", new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), null!, _testContract));
            Assert.That(ex.Message, Does.Contain("Phone number cannot be empty"));
        }

        [Test]
        public void AddRole_ShouldAddRoleToEmployee()
        {
            // Arrange
            var employee = CreateTestEmployee();
            var role = new CashierRole("cashier01", "SecurePass123!");

            // Act
            employee.AddRole(role);

            // Assert
            Assert.That(employee.Roles, Contains.Item(role));
        }

        [Test]
        public void AddRole_WithNullRole_ShouldThrowException()
        {
            // Arrange
            var employee = CreateTestEmployee();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => employee.AddRole(null!));
            Assert.That(ex.ParamName, Is.EqualTo("role"));
        }

        [Test]
        public void AddSubordinate_ShouldSetSupervisorRelationship()
        {
            // Arrange
            var supervisor = CreateTestEmployee("Supervisor");
            var subordinate = CreateTestEmployee("Subordinate");

            // Act
            supervisor.AddSubordinate(subordinate);

            // Assert
            Assert.That(supervisor.Subordinates, Contains.Item(subordinate));
            Assert.That(subordinate.Supervisor, Is.EqualTo(supervisor));
        }

        [Test]
        public void AddSubordinate_WithNullEmployee_ShouldThrowException()
        {
            // Arrange
            var supervisor = CreateTestEmployee();

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => supervisor.AddSubordinate(null!));
            Assert.That(ex.ParamName, Is.EqualTo("employee"));
        }

        [Test]
        public void AddSubordinate_SelfSupervision_ShouldThrowException()
        {
            // Arrange
            var employee = CreateTestEmployee();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => employee.AddSubordinate(employee));
            Assert.That(ex.Message, Does.Contain("Employee cannot supervise themselves"));
        }

        [Test]
        public void ChangeContract_ShouldUpdateContract()
        {
            // Arrange
            var employee = CreateTestEmployee();
            var newContract = new PartTimeContract(15.00m, 20);

            // Act
            employee.ChangeContract(newContract);

            // Assert
            Assert.That(employee.Contract, Is.EqualTo(newContract));
        }

        private Employee CreateTestEmployee(string firstName = "John")
        {
            return new Employee(
                firstName,
                "Doe",
                new DateOnly(1990, 1, 1),
                new DateOnly(2020, 1, 1),
                "123-456-7890",
                _testContract
            );
        }
    }
}