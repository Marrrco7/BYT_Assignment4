using System;
using System.Collections.Generic;
using System.Linq;
using Cinema.Core.models;

namespace Cinema.Core.models.session
{
    public enum SessionStatus
    {
        Scheduled,
        Started,
        Finished
    }

    public class Session
    {
        public static List<Session> All { get; } = new();

        private static int _counter = 0;

        public int Id { get; }

        public DateTime StartAt { get; set; }
        public DateTime EndAt => StartAt + Movie.Duration;

        public string Language { get; set; }
        public SessionStatus Status { get; set; }
        public Hall Hall { get; }
        public Movie Movie { get; }

        public Session(
            Hall hall,
            Movie movie,
            DateTime startAt,
            string language,
            SessionStatus status = SessionStatus.Scheduled)
        {
            Hall = hall ?? throw new ArgumentNullException(nameof(hall));
            Movie = movie ?? throw new ArgumentNullException(nameof(movie));

            if (string.IsNullOrWhiteSpace(language))
                throw new ArgumentException("Language cannot be empty.", nameof(language));

            Id = ++_counter;
            StartAt = startAt;
            Language = language;
            Status = status;
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
            string newLanguage,
            SessionStatus newStatus)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));

            session.StartAt = newStartAt;
            session.Language = newLanguage;
            session.Status = newStatus;
        }
        
        public void SaveSession()
        {
            AddSession(this);
        }

        // public void SaveEdit()
        // {
        // }
    }
}
