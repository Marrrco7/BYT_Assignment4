using System.Text.Json;
using Cinema.Core.models.customers; 
using Cinema.Core.models.sessions; 

namespace Cinema.Core.models.sales;

public class Review
{
    private int _ratingOfMovie;
    private int _ratingOfHall;
    
    public DateTime Date { get; set; }
    
    //is it optional?
    public string? Comment { get; set; }

    public int RatingOfMovie
    {
        get => _ratingOfMovie;
        private set
        {
            if (value < 1 || value > 5)
            {
                throw new ArgumentException("Movie rating must be an integer between 1 and 5.");
            }
            _ratingOfMovie = value;
        }
    }
    
    public int RatingOfHall
    {
        get => _ratingOfHall;
        set
        {
            if (value < 1 || value > 5)
            {
                throw new ArgumentException("Hall rating must be an integer between 1 and 5.");
            }
            _ratingOfHall = value; 
        }
    }
    
    public Customer Author { get; set; }
    public Movie ReviewedMovie { get; set; }
    
    private static readonly List<Review> _all = new();
    public static IReadOnlyList<Review> All => _all.AsReadOnly();
    
    public Review(int ratingOfMovie, int ratingOfHall, DateTime date, string? comment, Customer author, Movie reviewedMovie)
    {
        RatingOfMovie = ratingOfMovie;
        RatingOfHall = ratingOfHall;
        
        Date = date;
        Comment = comment;
        Author = author;
        ReviewedMovie = reviewedMovie;
        
        _all.Add(this);
    }
    
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