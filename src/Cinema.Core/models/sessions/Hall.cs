using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.operations;

namespace Cinema.Core.models.sessions;

public class Hall
{
    private static readonly int Capacity = 150;

    private string _name;
    public string Name => _name;

    // Qualified association: SeatId -> Seat
    private readonly Dictionary<int, Seat> _seatsById = new();

    private readonly List<Equipment> _equipment = new();
    private readonly List<Movie> _movies = new();

    private static readonly List<Hall> _all = new();
    public static IReadOnlyList<Hall> All => _all.AsReadOnly();

    public Hall(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty.", nameof(name));

        _name = name;
        _all.Add(this);
    }

    // ---------------------------------------------------------------------
    // Qualified Association: Hall contains Seats, key = SeatId
    // ---------------------------------------------------------------------

    public void AddSeat(Seat seat)
    {
        ArgumentNullException.ThrowIfNull(seat);

        int id = seat.SeatId;

        if (_seatsById.ContainsKey(id))
            throw new InvalidOperationException(
                $"Seat with id {id} already exists in Hall {Name}.");

        if (_seatsById.Count >= Capacity)
            throw new InvalidOperationException(
                $"Hall {Name} reached maximum capacity of {Capacity} seats.");

        _seatsById[id] = seat;

        // reverse connection
        seat.AttachToHallInternal(this);
    }

    public Seat? GetSeat(int seatId)
    {
        _seatsById.TryGetValue(seatId, out var seat);
        return seat;
    }

    public bool RemoveSeat(int seatId)
    {
        if (!_seatsById.TryGetValue(seatId, out var seat))
            return false;

        _seatsById.Remove(seatId);

        // reverse disconnect
        seat.DetachFromHallInternal(this);

        return true;
    }

    // ---------------------------------------------------------------------
    // Equipment
    // ---------------------------------------------------------------------

    public void AddEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        if (!_equipment.Contains(equipment))
            _equipment.Add(equipment);
    }

    // ---------------------------------------------------------------------
    // Movies
    // ---------------------------------------------------------------------

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

    // ---------------------------------------------------------------------
    // Persistence
    // ---------------------------------------------------------------------

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
