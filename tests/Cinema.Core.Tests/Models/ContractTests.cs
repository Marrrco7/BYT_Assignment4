using Cinema.Core.models.contract;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class ContractTests
    {
        [Test]
        public void FullTimeContract_WithValidData_ShouldCreateContract()
        {
            // Arrange
            var salary = 2500.00m;
            var hasBenefits = true;

            // Act
            var contract = new FullTimeContract(salary, hasBenefits);

            // Assert
            Assert.That(contract.Salary, Is.EqualTo(salary));
            Assert.That(contract.HasBenefitsPlan, Is.EqualTo(hasBenefits));
        }

        [Test]
        public void FullTimeContract_WithSalaryBelowMinimum_ShouldThrowException()
        {
            // Arrange
            var lowSalary = 400.00m; // Below MIN_SALARY of 500m

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new FullTimeContract(lowSalary, true));
            Assert.That(ex.Message, Does.Contain($"Salary must be at least {FullTimeContract.MIN_SALARY}"));
        }

        [Test]
        public void FullTimeContract_WithSalaryAboveMaximum_ShouldThrowException()
        {
            // Arrange
            var highSalary = 4000.00m; // Above MAX_SALARY of 3500m

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new FullTimeContract(highSalary, true));
            Assert.That(ex.Message, Does.Contain($"Salary cannot exceed {FullTimeContract.MAX_SALARY}"));
        }

        [Test]
        public void FullTimeContract_WithTooManyDecimals_ShouldThrowException()
        {
            // Arrange
            var salary = 2500.123m; // More than 2 decimal places

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new FullTimeContract(salary, true));
            Assert.That(ex.Message, Does.Contain("Salary must have at most two decimal places"));
        }

        [Test]
        public void PartTimeContract_WithValidData_ShouldCreateContract()
        {
            // Arrange
            var hourlyRate = 15.00m;
            var maxHours = 20;

            // Act
            var contract = new PartTimeContract(hourlyRate, maxHours);

            // Assert
            Assert.That(contract.HourlyRate, Is.EqualTo(hourlyRate));
            Assert.That(contract.MaxWeekHours, Is.EqualTo(maxHours));
        }

        [Test]
        public void PartTimeContract_WithRateBelowMinimum_ShouldThrowException()
        {
            // Arrange
            var lowRate = 4.00m; // Below MIN_HOURLY_RATE of 5m

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PartTimeContract(lowRate, 20));
            Assert.That(ex.Message, Does.Contain($"Hourly rate must be between {PartTimeContract.MIN_HOURLY_RATE}"));
        }

        [Test]
        public void PartTimeContract_WithHoursAboveMaximum_ShouldThrowException()
        {
            // Arrange
            var highHours = 35; // Above MAX_WEEKLY_HOURS of 30

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() =>
                new PartTimeContract(15.00m, highHours));
            Assert.That(ex.Message, Does.Contain($"Weekly hours must be between {PartTimeContract.MIN_WEEKLY_HOURS}"));
        }
    }
}