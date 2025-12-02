using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.roles;
using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions;

public class Session
{
    // Static extent

    private static readonly List<Session> _all = new();
    public static IReadOnlyList<Session> All => _all.AsReadOnly();

    // Fields

    [JsonIgnore]
    private readonly List<Review> _reviews = new();

    [JsonIgnore]
    private readonly List<TechnicianRole> _technicians = new();

    // Properties

    public DateTime StartAt { get; set; }

    public string Language { get; set; }

    public Hall Hall { get; private set; }

    public Movie Movie { get; private set; }

    [JsonIgnore]
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();

  
    [JsonIgnore]
    public IReadOnlyList<TechnicianRole> Technicians => _technicians.AsReadOnly();

    // Constructors

    public Session()
    {
    }

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

        _all.Add(this);
    }

    // Associations: Reviews (Customer–Session via Review)

    internal void AddReviewInternal(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (!_reviews.Contains(review))
            _reviews.Add(review);
    }

    // Associations

    public void AddTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (_technicians.Contains(technician))
            return;

        _technicians.Add(technician);
        technician.AddSessionInternal(this); //  reverse connection
    }

    public void RemoveTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (_technicians.Count <= 1)
            throw new InvalidOperationException(
                "Session must have at least one technician (1..* multiplicity).");

        if (_technicians.Remove(technician))
        {
            technician.RemoveSessionInternal(this); //  reverse connection
        }
    }

    internal void AddTechnicianInternal(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (!_technicians.Contains(technician))
            _technicians.Add(technician);
    }

    internal void RemoveTechnicianInternal(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        _technicians.Remove(technician);
    }

    // Session extent

    public static IReadOnlyList<Session> ListOfSessions()
    {
        return All;
    }

    public static void AddSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!_all.Contains(session))
            _all.Add(session);
    }

    public static bool DeleteSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        return _all.Remove(session);
    }

    public static void EditSession(
        Session session,
        DateTime newStartAt,
        string newLanguage)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (string.IsNullOrWhiteSpace(newLanguage))
            throw new ArgumentException("Language cannot be empty.", nameof(newLanguage));

        session.StartAt = newStartAt;
        session.Language = newLanguage;
    }

    public void SaveSession()
    {
        AddSession(this);
    }

    // Business logic

    public DateTime CalculateEndAt()
    {
        return StartAt + Movie.Duration;
    }

    // Persistence

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

        _all.Clear();
        if (sessions != null)
            _all.AddRange(sessions);
    }
}
