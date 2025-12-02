using System.Text.Json.Serialization;

namespace Cinema.Core.models.sessions;

public class Movie
{
    private static List<Movie> All { get; } = new();
    private string _title;

    public string Title
    {
        get => _title;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Title cannot be empty.", nameof(value));
            _title = value;
        }
    }
    private TimeSpan _duration;

    public TimeSpan Duration
    {
        get => _duration;
        set
        {
            if (value <= TimeSpan.Zero)
                throw new ArgumentException("Duration must be positive.", nameof(value));
            _duration = value;
        }
    }
    public List<string> Genres { get; }
    private int? _ageRestriction;

    public int? AgeRestriction
    {
        get => _ageRestriction;
        set
        {
            if (value is < 0)
                throw new ArgumentException("Age restriction must be non-negative.", nameof(value));
            _ageRestriction = value;
        }
    }
    
    // Associations
    [JsonIgnore] private readonly List<Session> _sessions = new();
    [JsonIgnore] public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();

    public Movie(
        string title,
        TimeSpan duration,
        IEnumerable<string> genres,
        int? ageRestriction = null)
    {

        if (genres == null)
            throw new ArgumentNullException(nameof(genres));

        var genreList = genres
            .Where(g => !string.IsNullOrWhiteSpace(g))
            .ToList();

        if (genreList.Count == 0)
            throw new ArgumentException("Movie must have at least one genre.", nameof(genres));

        Title = title;
        Duration = duration;
        Genres = genreList;
        AgeRestriction = ageRestriction;

        All.Add(this);
    }

    // Business logic

    public static IReadOnlyList<Movie> ListOfAllMovies()
    {
        return All.AsReadOnly();
    }

    public static Movie? SearchMovieByTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return null;

        return All.FirstOrDefault(
            m => string.Equals(m.Title, title, StringComparison.OrdinalIgnoreCase));
    }

    public void AssignMovieToHall(Hall hall)
    {
        if (hall == null) throw new ArgumentNullException(nameof(hall));
        hall.AddMovie(this);
    }


    // Session
    internal void AddSessionInternal(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!_sessions.Contains(session))
            _sessions.Add(session);
    }

    internal void RemoveSessionInternal(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Remove(session);
    }
}