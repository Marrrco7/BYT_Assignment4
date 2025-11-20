using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.operations;

namespace Cinema.Core.models.sessions;

public class Hall
{
    private static readonly int Capacity = 150;
    private string Name { get; set; }
    
    private readonly Dictionary<int, Seat> _seatsByNumber = new();
    
    private readonly List<Equipment> _equipment = new();
    
    private readonly List<Movie> _movies = new();
    
    private static readonly List<Hall> _all = new();
    private static IReadOnlyList<Hall> All => _all.AsReadOnly();

    public Hall(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty.", nameof(name));

        Name = name;
    }

    public void AddSeat(int seatNumber, Seat seat)
    {
        ArgumentNullException.ThrowIfNull(seat);

        if (seatNumber <= 0)
            throw new ArgumentException("Seat number must be positive.");

        if (_seatsByNumber.ContainsKey(seatNumber))
            throw new InvalidOperationException($"Seat {seatNumber} already exists in Hall {Name}.");

        if (_seatsByNumber.Count >= Capacity)
            throw new InvalidOperationException(
                $"Hall {Name} reached maximum capacity of {Capacity} seats.");

        _seatsByNumber[seatNumber] = seat;
    }
    
    public Seat? GetSeat(int seatNumber)
    {
        _seatsByNumber.TryGetValue(seatNumber, out var seat);
        return seat;
    }


    public void AddEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));
        _equipment.Add(equipment);
    }
    

    public void AddMovie(Movie movie)
    {
        if (movie == null) throw new ArgumentNullException(nameof(movie));
        _movies.Add(movie);  
    }

    public void RemoveMovie(Movie movie)
    {
        if (movie == null) throw new ArgumentNullException(nameof(movie));
        _movies.Remove(movie);
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
        var halls = JsonSerializer.Deserialize<List<Hall>>(json, new JsonSerializerOptions
        {
            ReferenceHandler = ReferenceHandler.Preserve
        });

        _all.Clear();
        if (halls != null)
            _all.AddRange(halls);
    }

    
}