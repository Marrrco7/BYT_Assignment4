using Cinema.Core.models.operations;

namespace Cinema.Core.models.sessions;

public class Hall
{
    
    private static int _MAX_CAPACITY = 150;
    private static int _counter = 0;
    
    public static List<Hall> All { get; } = new();
    
    private int Id { get;}
    
    public string Name { get; private set; }
    
    public int Capacity { get; private set; }
    
    private readonly Dictionary<int, Seat> _seatsByNumber = new();
    public IReadOnlyCollection<Seat> Seats => _seatsByNumber.Values;
    
    private readonly List<Equipment> _equipment = new();
    public IReadOnlyList<Equipment> Equipment => _equipment.AsReadOnly();
    
    private readonly List<Movie> _movies = new();
    public IReadOnlyList<Movie> Movies => _movies.AsReadOnly();
    
    public Hall(string name, int capacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty.", nameof(name));
        if (capacity <= 0 || capacity > _MAX_CAPACITY)
            throw new ArgumentException("Capacity must be positive or lower than ", nameof(_MAX_CAPACITY));

        Id = ++_counter;
        Name = name;
        Capacity  = capacity;
        
        All.Add(this);
    }
    
    public static IReadOnlyList<Hall> ListOfHalls() => All.AsReadOnly();
    
    public void AddSeat(int seatNumber, Seat seat)
    {
        ArgumentNullException.ThrowIfNull(seat);
        if (seatNumber <= 0)
            throw new ArgumentException("Seat number must be positive.", nameof(seatNumber));

        if (_seatsByNumber.ContainsKey(seatNumber))
            throw new InvalidOperationException(
                $"Seat number {seatNumber} already exists in hall {Name}.");

        if (_seatsByNumber.Count >= _MAX_CAPACITY)
            throw new InvalidOperationException(
                $"Hall {Name} is at full capacity ({_MAX_CAPACITY} seats).");

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


}