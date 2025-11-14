using Cinema.Core.models.sessions;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class TicketTests
    {
        private Session _testSession = null!;
        private Seat _testSeat = null!;

        [SetUp]
        public void Setup()
        {
            Ticket.All.Clear();

            var hall = new Hall("Test Hall", 100);
            var movie = new Movie("Test Movie", TimeSpan.FromHours(2), new[] { "Action" });
            _testSession = new Session(hall, movie, DateTime.Now.AddDays(1), "English");
            _testSeat = new Seat(SeatType.Normal, 10.00m, false);
        }

        [Test]
        public void Constructor_WithValidData_ShouldCreateTicket()
        {
            // Arrange
            var discount = 2.00m;
            var bonusPoints = 10;

            // Act
            var ticket = new Ticket(_testSession, _testSeat, discount, bonusPoints);

            // Assert
            Assert.That(ticket.Session, Is.EqualTo(_testSession));
            Assert.That(ticket.Seat, Is.EqualTo(_testSeat));
            Assert.That(ticket.DiscountAmount, Is.EqualTo(discount));
            Assert.That(ticket.BonusPointsUsed, Is.EqualTo(bonusPoints));
            Assert.That(ticket.IsBooked, Is.False);
        }

        [Test]
        public void Constructor_WithNullSession_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new Ticket(null!, _testSeat));
            Assert.That(ex.ParamName, Is.EqualTo("session"));
        }

        [Test]
        public void Constructor_WithNullSeat_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new Ticket(_testSession, null!));
            Assert.That(ex.ParamName, Is.EqualTo("seat"));
        }

        [Test]
        public void Constructor_WithNegativeDiscount_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Ticket(_testSession, _testSeat, discountAmount: -5.00m));
            Assert.That(ex.Message, Does.Contain("Discount cannot be negative"));
        }

        [Test]
        public void Constructor_WithNegativeBonusPoints_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Ticket(_testSession, _testSeat, bonusPointsUsed: -10));
            Assert.That(ex.Message, Does.Contain("Bonus points cannot be negative"));
        }

        [Test]
        public void FinalPrice_ShouldCalculateCorrectly()
        {
            // Arrange
            var seatPrice = 10.00m;
            var discount = 2.00m;
            var bonusPoints = 5;
            var seat = new Seat(SeatType.Normal, seatPrice, false);
            var ticket = new Ticket(_testSession, seat, discount, bonusPoints);

            // Act
            var finalPrice = ticket.FinalPrice;

            // Assert
            var expectedPrice = seatPrice - discount - bonusPoints;
            Assert.That(finalPrice, Is.EqualTo(expectedPrice));
        }

        [Test]
        public void FinalPrice_ShouldNotGoBelowZero()
        {
            // Arrange
            var seatPrice = 5.00m;
            var discount = 10.00m; // More than seat price
            var seat = new Seat(SeatType.Normal, seatPrice, false);
            var ticket = new Ticket(_testSession, seat, discount);

            // Act
            var finalPrice = ticket.FinalPrice;

            // Assert
            Assert.That(finalPrice, Is.EqualTo(0));
        }

        [Test]
        public void VipSeat_FinalPrice_ShouldApplyMultiplier()
        {
            // Arrange
            var normalPrice = 10.00m;
            var vipSeat = new Seat(SeatType.Vip, normalPrice, false);
            var ticket = new Ticket(_testSession, vipSeat);

            // Act
            var finalPrice = ticket.FinalPrice;
            var expectedVipPrice = normalPrice * 1.8m;

            // Assert
            Assert.That(vipSeat.FinalSeatPrice, Is.EqualTo(expectedVipPrice));
        }

        [Test]
        public void NormalSeat_FinalSeatPrice_ShouldReturnNormalPrice()
        {
            // Arrange
            var normalPrice = 10.00m;
            var normalSeat = new Seat(SeatType.Normal, normalPrice, false);
            var ticket = new Ticket(_testSession, normalSeat);

            // Act
            var finalPrice = normalSeat.FinalSeatPrice;

            // Assert
            Assert.That(finalPrice, Is.EqualTo(normalPrice));
        }

        [Test]
        public void BookTicket_ShouldSetIsBookedToTrue()
        {
            // Arrange
            var ticket = new Ticket(_testSession, _testSeat);

            // Act
            ticket.BookTicket();

            // Assert
            Assert.That(ticket.IsBooked, Is.True);
        }

        [Test]
        public void BookTicket_WhenAlreadyBooked_ShouldThrowException()
        {
            // Arrange
            var ticket = new Ticket(_testSession, _testSeat);
            ticket.BookTicket();

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => ticket.BookTicket());
            Assert.That(ex.Message, Does.Contain("Ticket is already booked"));
        }

        [Test]
        public void StaticExtent_ShouldStoreTicketsCorrectly()
        {
            // Arrange
            var initialCount = Ticket.All.Count;

            // Act
            var ticket1 = new Ticket(_testSession, _testSeat);
            var ticket2 = new Ticket(_testSession, _testSeat);

            // Assert
            Assert.That(Ticket.All.Count, Is.EqualTo(initialCount + 2));
            Assert.That(Ticket.ListAll(), Contains.Item(ticket1));
        }
    }
}