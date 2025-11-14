using Cinema.Core.models.sessions;

namespace Cinema.Tests.Models
{
    [TestFixture]
    public class SessionTests
    {
        private Hall _testHall = null!;
        private Movie _testMovie = null!;

        [SetUp]
        public void Setup()
        {
            Session.All.Clear();

            _testHall = new Hall("Test Hall", 100);
            _testMovie = new Movie("Test Movie", TimeSpan.FromHours(2), new[] { "Action", "Drama" }, 16);
        }

        [Test]
        public void Constructor_WithValidData_ShouldCreateSession()
        {
            // Arrange
            var startTime = DateTime.Now.AddDays(1);
            var language = "English";

            // Act
            var session = new Session(_testHall, _testMovie, startTime, language);

            // Assert
            Assert.That(session.Hall, Is.EqualTo(_testHall));
            Assert.That(session.Movie, Is.EqualTo(_testMovie));
            Assert.That(session.StartAt, Is.EqualTo(startTime));
            Assert.That(session.Language, Is.EqualTo(language));
            Assert.That(session.Status, Is.EqualTo(SessionStatus.Scheduled));
        }

        [Test]
        public void Constructor_WithNullHall_ShouldThrowException()
        {
            // Arrange
            var startTime = DateTime.Now.AddDays(1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new Session(null!, _testMovie, startTime, "English"));
            Assert.That(ex.ParamName, Is.EqualTo("hall"));
        }

        [Test]
        public void Constructor_WithNullMovie_ShouldThrowException()
        {
            // Arrange
            var startTime = DateTime.Now.AddDays(1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                new Session(_testHall, null!, startTime, "English"));
            Assert.That(ex.ParamName, Is.EqualTo("movie"));
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        public void Constructor_WithEmptyLanguage_ShouldThrowException(string invalidLanguage)
        {
            // Arrange
            var startTime = DateTime.Now.AddDays(1);

            // Act & Assert
            var ex = Assert.Throws<ArgumentException>(() =>
                new Session(_testHall, _testMovie, startTime, invalidLanguage));
            Assert.That(ex.Message, Does.Contain("Language cannot be empty"));
        }

        [Test]
        public void EndAt_ShouldBeCalculatedCorrectly()
        {
            // Arrange
            var startTime = new DateTime(2025, 1, 1, 10, 0, 0);
            var session = new Session(_testHall, _testMovie, startTime, "English");

            // Act
            var endAt = session.EndAt;

            // Assert
            var expectedEnd = startTime + _testMovie.Duration;
            Assert.That(endAt, Is.EqualTo(expectedEnd));
        }

        [Test]
        public void StaticExtent_ShouldStoreSessionsCorrectly()
        {
            // Arrange
            var initialCount = Session.All.Count;
            var session1 = CreateTestSession();
            var session2 = CreateTestSession();

            // Act
            Session.AddSession(session1);
            Session.AddSession(session2);

            // Assert
            Assert.That(Session.All.Count, Is.EqualTo(initialCount + 2));
            Assert.That(Session.ListOfSessions(), Contains.Item(session1));
        }

        [Test]
        public void AddSession_WithNullSession_ShouldThrowException()
        {
            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() => Session.AddSession(null!));
            Assert.That(ex.ParamName, Is.EqualTo("session"));
        }

        [Test]
        public void DeleteSession_ShouldRemoveSessionFromExtent()
        {
            // Arrange
            var session = CreateTestSession();
            Session.AddSession(session);
            var initialCount = Session.All.Count;

            // Act
            var result = Session.DeleteSession(session);

            // Assert
            Assert.That(result, Is.True);
            Assert.That(Session.All.Count, Is.EqualTo(initialCount - 1));
            Assert.That(Session.All, Does.Not.Contain(session));
        }

        [Test]
        public void EditSession_ShouldUpdateSessionProperties()
        {
            // Arrange
            var session = CreateTestSession();
            var newStartTime = DateTime.Now.AddDays(2);
            var newLanguage = "Polish";
            var newStatus = SessionStatus.Started;

            // Act
            Session.EditSession(session, newStartTime, newLanguage, newStatus);

            // Assert
            Assert.That(session.StartAt, Is.EqualTo(newStartTime));
            Assert.That(session.Language, Is.EqualTo(newLanguage));
            Assert.That(session.Status, Is.EqualTo(newStatus));
        }

        [Test]
        public void SaveSession_ShouldAddSessionToExtent()
        {
            // Arrange - Clear any sessions added by constructor or other tests
            var session = new Session(_testHall, _testMovie, DateTime.Now.AddDays(1), "English");
            Session.All.Clear();
            var countBefore = Session.All.Count; // Should be 0

            // Act - SaveSession should add it back
            session.SaveSession();

            // Assert - Should now have 1 session
            Assert.That(Session.All.Count, Is.EqualTo(countBefore + 1));
            Assert.That(Session.All, Contains.Item(session), "SaveSession should add session to extent");
        }

        private Session CreateTestSession()
        {
            return new Session(_testHall, _testMovie, DateTime.Now.AddDays(1), "English");
        }
    }
}