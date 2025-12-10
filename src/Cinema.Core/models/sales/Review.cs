using System.Text.Json;
using Cinema.Core.models.customers;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.sales;

public class Review
{
    // Static extent

    private static readonly List<Review> _all = new();
    public static IReadOnlyList<Review> All => _all.AsReadOnly();

    // Fields

    private int _ratingOfMovie;
    private int _ratingOfHall;

    private Customer? _author;
    private Session? _reviewedSession;

    // Properties

    public DateTime Date { get; private set; }

    public string Comment { get; private set; }

    public int RatingOfMovie
    {
        get => _ratingOfMovie;
        private set
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Movie rating must be an integer between 1 and 5.");

            _ratingOfMovie = value;
        }
    }

    public int RatingOfHall
    {
        get => _ratingOfHall;
        private set
        {
            if (value < 1 || value > 5)
                throw new ArgumentException("Hall rating must be an integer between 1 and 5.");

            _ratingOfHall = value;
        }
    }

    public bool IsDeleted { get; private set; }

    public Customer? Author => _author;

    public Session? ReviewedSession => _reviewedSession;

    // Constructor

    public Review(
        int ratingOfMovie,
        int ratingOfHall,
        DateTime date,
        string comment,
        Customer author,
        Session reviewedSession)
    {
        RatingOfMovie = ratingOfMovie;
        RatingOfHall  = ratingOfHall;
        Date          = date;
        Comment       = comment ?? string.Empty;

        if (author == null)
            throw new ArgumentNullException(nameof(author));
        if (reviewedSession == null)
            throw new ArgumentNullException(nameof(reviewedSession));

        // устанавливаем связи через методы с reverse connection
        SetAuthor(author);
        SetReviewedSession(reviewedSession);

        _all.Add(this);
    }

    // Ассоциация с Customer

    public void SetAuthor(Customer? customer)
    {
        if (_author == customer)
            return;

        if (_author != null)
        {
            var oldAuthor = _author;

            if (oldAuthor.Reviews.Contains(this))
            {
                oldAuthor.RemoveReview(this);
            }
        }

        if (customer != null)
        {
            _author = customer;

            if (!customer.Reviews.Contains(this))
            {
                customer.AddReview(this);
            }
        }
        else
        {
            _author = null;
        }
    }

    // Ассоциация с Session

    public void SetReviewedSession(Session? session)
    {
        if (_reviewedSession == session)
            return;

        if (_reviewedSession != null)
        {
            var oldSession = _reviewedSession;

            if (oldSession.Reviews.Contains(this))
            {
                oldSession.RemoveReview(this);
            }
        }

        if (session != null)
        {
            _reviewedSession = session;

            if (!session.Reviews.Contains(this))
            {
                session.AddReview(this);
            }
        }
        else
        {
            _reviewedSession = null;
        }
    }

    // Business logic

    public void Edit(int newMovieRating, int newHallRating, string newComment)
    {
        RatingOfMovie = newMovieRating;
        RatingOfHall  = newHallRating;
        Comment       = newComment ?? string.Empty;
        Date          = DateTime.Now;
    }

    public void Delete()
    {
        IsDeleted = true;
    }

    // Persistence

    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(All, options);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);
        var reviews = JsonSerializer.Deserialize<List<Review>>(json);

        _all.Clear();
        if (reviews != null)
        {
            _all.AddRange(reviews);
        }
    }
}
