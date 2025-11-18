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
        get { return _ratingOfMovie; }
        set
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
        get { return _ratingOfHall; }
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
    
    public static List<Review> All { get; set; } = new();
    
    public Review(int ratingOfMovie, int ratingOfHall, DateTime date, string? comment, Customer author, Movie reviewedMovie)
    {
        RatingOfMovie = ratingOfMovie;
        RatingOfHall = ratingOfHall;
        
        Date = date;
        Comment = comment;
        Author = author;
        ReviewedMovie = reviewedMovie;
        
        All.Add(this);
    }
    
    public Review(){}
    
    //we have to test it
    public static void SaveExtent()
    {
        string jsonString = JsonSerializer.Serialize(All, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText("reviews.json", jsonString);
    }

    //we have to test it
    public static void LoadExtent()
    {
        if (File.Exists("reviews.json"))
        {
            string jsonString = File.ReadAllText("reviews.json");
            All = JsonSerializer.Deserialize<List<Review>>(jsonString) ?? new List<Review>();
        }
    }
}