using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.operations;

namespace Cinema.Core.models.sessions;

public class Hall
{
    private static readonly int Capacity = 150;

    private string _name; 
    public string Name 
    { 
        get => _name; 
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Hall name cannot be empty.", nameof(value));
            _name = value;
        }
    }
    
    private readonly Dictionary<int, Seat> _seatsById = new();
    
    private readonly List<Movie> _movies = new();
    
    private static readonly List<Hall> _all = new();
    private static IReadOnlyList<Hall> All => _all.AsReadOnly();
    
    // ===== Associations =====
    [JsonIgnore] private readonly List<Shift> _shifts = new();
    [JsonIgnore] private readonly List<Session> _sessions = new();
    [JsonIgnore] private readonly List<Equipment> _equipment = new();
    
    public IReadOnlyList<Shift> Shifts => _shifts.AsReadOnly();
    public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();
    public IReadOnlyList<Equipment> Equipment => _equipment.AsReadOnly();

    // ===== Constructor =====
    public Hall(string name)
    {
        Name = name;
        _all.Add(this);
    }
    
    // ===== Qualified Association: SEATS =====

    public void AddSeat(Seat seat)
    {
        ArgumentNullException.ThrowIfNull(seat);

        if (_seatsById.ContainsKey(seat.Id))
            throw new InvalidOperationException(
                $"Seat with Id {seat.Id} already exists in Hall {Name}.");

        if (_seatsById.Count >= Capacity)
            throw new InvalidOperationException(
                $"Hall {Name} reached maximum capacity of {Capacity} seats.");

        _seatsById[seat.Id] = seat;
    }

    public Seat? GetSeat(int seatId)
    {
        _seatsById.TryGetValue(seatId, out var seat);
        return seat;
    }

    public void RemoveSeat(int seatId)
    {
        _seatsById.Remove(seatId);
    }

    // ===== Movies =====
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
    
    // ===== Persistence =====
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
        var halls = JsonSerializer.Deserialize<List<Hall>>(json, 
            new JsonSerializerOptions { ReferenceHandler = ReferenceHandler.Preserve });

        _all.Clear();
        if (halls != null)
            _all.AddRange(halls);
    }
    
    // ===== Shift =====
    public void AddShift(Shift shift)
    {
        if (shift == null)
            throw new ArgumentNullException(nameof(shift));

        if (_shifts.Contains(shift)) return;

        _shifts.Add(shift);

        if (shift.Hall != this)
            shift.SetHall(this);
    }

    public void RemoveShift(Shift shift)
    {
        if (shift == null)
            throw new ArgumentNullException(nameof(shift));

        _shifts.Remove(shift);
    }
    
    // ===== Session =====
    public void AddSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Contains(session)) return;

        _sessions.Add(session);

        if (session.Hall != this)
            session.SetHall(this);
    }

    public void RemoveSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Remove(session);
    }
    
    // ===== Equipment =====
    public void AddEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        if (_equipment.Contains(equipment)) return;

        _equipment.Add(equipment);

        if (equipment.Hall != this)
            equipment.SetHall(this);
    }

    public void RemoveEquipment(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        _equipment.Remove(equipment);
    }
}
