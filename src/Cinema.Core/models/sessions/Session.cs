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

    [JsonIgnore]
    private readonly List<Ticket> _tickets = new();

    [JsonIgnore]
    private readonly List<Promotion> _promotions = new();

    private readonly bool _isDummy;

    // Properties

    public DateTime StartAt { get; set; }

    private string _language = null!;

    public string Language
    {
        get => _language;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Language cannot be empty.", nameof(value));

            _language = value;
        }
    }

    private Hall _hall = null!;
    public Hall Hall
    {
        get => _hall;
        private set => _hall = value;
    }

    private Movie _movie = null!;
    public Movie Movie
    {
        get => _movie;
        private set => _movie = value;
    }

    public bool IsDeleted { get; private set; }

    [JsonIgnore]
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<TechnicianRole> Technicians => _technicians.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<Promotion> Promotions => _promotions.AsReadOnly();

    [JsonIgnore]
    public bool IsDummy => _isDummy;


    public Session() { }

    private Session(bool isDummy)
    {
        _isDummy  = isDummy;
        StartAt   = DateTime.MinValue;
        _language = "DUMMY";
    }

    public Session(
        Hall hall,
        Movie movie,
        DateTime startAt,
        string language)
    {
        if (hall == null)  throw new ArgumentNullException(nameof(hall));
        if (movie == null) throw new ArgumentNullException(nameof(movie));

        _isDummy = false;

        StartAt  = startAt;
        Language = language;

        SetHall(hall);
        SetMovie(movie);

        var dummyTech = TechnicianRole.CreateDummyForSession(this);
        _technicians.Add(dummyTech);

        _all.Add(this);
    }

    public static Session CreateDummyForTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        var dummy = new Session(isDummy: true);

        dummy.AttachTechnicianDummy(technician);
        technician.AttachSessionDummy(dummy);

        return dummy;
    }

    // ------------ Associations: Reviews 

    public void AddReview(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (_reviews.Contains(review))
        {
            if (review.ReviewedSession != this)
            {
                review.SetReviewedSession(this);
            }
            return;
        }

        _reviews.Add(review);

        if (review.ReviewedSession != this)
        {
            review.SetReviewedSession(this);
        }
    }

    public void RemoveReview(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (!_reviews.Contains(review))
            return;

        _reviews.Remove(review);

        if (review.ReviewedSession == this)
        {
            review.SetReviewedSession(null);
        }
    }

    // ------------ Associations: Hall 

    public void SetHall(Hall newHall)
    {
        if (newHall == null)
            throw new ArgumentNullException(nameof(newHall), "Session must be assigned to a Hall.");

        if (_hall == newHall)
            return;

        if (_hall != null && _hall.Sessions.Contains(this))
        {
            _hall.RemoveSession(this);
        }

        _hall = newHall;

        if (!_hall.Sessions.Contains(this))
        {
            _hall.AddSession(this);
        }
    }

    // ------------ Associations: Movie 

    public void SetMovie(Movie newMovie)
    {
        if (newMovie == null)
            throw new ArgumentNullException(nameof(newMovie), "Session must specify a Movie.");

        if (_movie == newMovie)
            return;

        if (_movie != null && _movie.Sessions.Contains(this))
        {
            _movie.RemoveSession(this);
        }

        _movie = newMovie;

        if (!_movie.Sessions.Contains(this))
        {
            _movie.AddSession(this);
        }
    }

    // ------------ Associations: Technicians

    public void AddTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (_technicians.Contains(technician))
        {
            if (!technician.Sessions.Contains(this))
            {
                technician.AssignToSession(this);
            }
            return;
        }

        _technicians.Add(technician);

        if (!technician.Sessions.Contains(this))
        {
            technician.AssignToSession(this);
        }
    }

    public void RemoveTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (technician.IsDummy)
            return;

        if (!_technicians.Contains(technician))
            return;

        _technicians.Remove(technician);

        if (technician.Sessions.Contains(this))
        {
            technician.RemoveFromSession(this);
        }
    }
    public void AttachTechnicianDummy(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (!_technicians.Contains(technician))
        {
            _technicians.Add(technician);
        }
    }

    // ------------ Associations: Tickets 

    public void AddTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (_tickets.Contains(ticket))
        {
            if (ticket.Session != this)
            {
                ticket.SetSession(this);
            }
            return;
        }

        _tickets.Add(ticket);

        if (ticket.Session != this)
        {
            ticket.SetSession(this);
        }
    }

    public void RemoveTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (!_tickets.Contains(ticket))
            return;

        _tickets.Remove(ticket);

        if (ticket.Session == this)
        {
            ticket.SetSession(null);
        }
    }

    // ------------ Associations: Promotions --

    public void AddPromotion(Promotion promotion)
    {
        if (promotion == null)
            throw new ArgumentNullException(nameof(promotion));

        if (_promotions.Contains(promotion))
        {
            if (!promotion.Sessions.Contains(this))
            {
                promotion.AddSession(this);
            }
            return;
        }

        _promotions.Add(promotion);

        if (!promotion.Sessions.Contains(this))
        {
            promotion.AddSession(this);
        }
    }

    public void RemovePromotion(Promotion promotion)
    {
        if (promotion == null)
            throw new ArgumentNullException(nameof(promotion));

        if (!_promotions.Contains(promotion))
            return;

        _promotions.Remove(promotion);

        if (promotion.Sessions.Contains(this))
        {
            promotion.RemoveSession(this);
        }
    }


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

    public void DeleteSession()
    {
        IsDeleted = true;
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

        session.StartAt  = newStartAt;
        session.Language = newLanguage;
    }

    public void SaveSession()
    {
        AddSession(this);
    }

    // ------------ Business logic ------------

    public DateTime CalculateEndAt()
    {
        return StartAt + Movie.Duration;
    }

    // ------------ Persistence ------------

    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented    = true,
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
