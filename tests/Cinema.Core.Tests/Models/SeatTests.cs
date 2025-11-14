using Cinema.Core.models.sessions;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class SeatTests
    {
        [Test]
        public void Constructor_WithValidData_ShouldCreateSeat()
        {
            // Arrange
            var type = SeatType.Normal;
            var price = 15.00m;
            var isAccessible = true;

            // Act
            var seat = new Seat(type, price, isAccessible);

            // Assert
            Assert.That(seat.Type, Is.EqualTo(type));
            Assert.That(seat.NormalPrice, Is.EqualTo(price));
            Assert.That(seat.IsAccessible, Is.EqualTo(isAccessible));
            Assert.That(seat.TicketMultiplier, Is.EqualTo(1.8m));
        }

        [Test]
        public void Constructor_WithNegativePrice_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Seat(SeatType.Normal, -5.00m, false));
            Assert.That(ex.Message, Does.Contain("Price cannot be negative"));
        }

        [Test]
        public void VipSeat_FinalSeatPrice_ShouldApplyMultiplier()
        {
            // Arrange
            var normalPrice = 10.00m;
            var vipSeat = new Seat(SeatType.Vip, normalPrice, false);

            // Act
            var finalPrice = vipSeat.FinalSeatPrice;

            // Assert
            Assert.That(finalPrice, Is.EqualTo(normalPrice * 1.8m));
        }

        [Test]
        public void NormalSeat_FinalSeatPrice_ShouldReturnNormalPrice()
        {
            // Arrange
            var normalPrice = 10.00m;
            var normalSeat = new Seat(SeatType.Normal, normalPrice, false);

            // Act
            var finalPrice = normalSeat.FinalSeatPrice;

            // Assert
            Assert.That(finalPrice, Is.EqualTo(normalPrice));
        }

        [Test]
        public void Constructor_WithCustomMultiplier_ShouldUseCustomValue()
        {
            // Arrange
            var customMultiplier = 2.0m;

            // Act
            var seat = new Seat(SeatType.Vip, 10.00m, false, customMultiplier);

            // Assert
            Assert.That(seat.TicketMultiplier, Is.EqualTo(customMultiplier));
            Assert.That(seat.FinalSeatPrice, Is.EqualTo(20.00m));
        }
    }
}