using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cinema.Core.models.sessions
{
    public class Session
    {
        public static List<Session> All { get; } = new();
        public DateTime StartAt { get; set; }
        public string Language { get; set; }
        public Hall Hall { get; }
        public Movie Movie { get; }

        public Session(
            Hall hall,
            Movie movie,
            DateTime startAt,
            string language)
        {
            Hall = hall ?? throw new ArgumentNullException(nameof(hall));
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));

            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("Language cannot be empty.", nameof(language));
            
            StartAt = startAt;
            Language = language;

            All.Add(this);
        }
        
        public static IReadOnlyList<Session> ListOfSessions()
        {
            return All.AsReadOnly();
        }

        public static void AddSession(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            if (!All.Contains(session))
                All.Add(session);
        }

        public static bool DeleteSession(Session session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            return All.Remove(session);
        }

        public static void EditSession(
            Session session,
            DateTime newStartAt,
            string newLanguage)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.StartAt = newStartAt;
            session.Language = newLanguage;
        }
        
        public void SaveSession()
        {
            AddSession(this);
        }

        public DateTime CalculateEndAt()
        {
            return StartAt + Movie.Duration;

        }

        public static void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var json = JsonSerializer.Serialize(All, options);
            File.WriteAllText(filePath, json);
        }

        public static void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var sessions = JsonSerializer.Deserialize<List<Session>>(json, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });

            All.Clear();
            if (sessions != null)
                All.AddRange(sessions);
        }

    }
}
