using Cinema.Core.models.operations;

namespace Cinema.Core.models.session;

public class Hall
{
    private static int _counter = 0;
    
    private int Id { get;}
    
    public string Name { get; private set; }
    
    public int MaxCapacity { get; private set; }
    
    private readonly Dictionary<int, Seat> _seatsByNumber = new();
    public IReadOnlyCollection<Seat> Seats => _seatsByNumber.Values;
    
    private readonly List<Equipment> _equipment = new();
    public IReadOnlyList<Equipment> Equipment => _equipment.AsReadOnly();
    
    private readonly List<Movie> _movies = new();
    public IReadOnlyList<Movie> Movies => _movies.AsReadOnly();
    
    public Hall(string name, int maxCapacity = 150)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Hall name cannot be empty.", nameof(name));
        if (maxCapacity <= 0)
            throw new ArgumentException("Capacity must be positive.", nameof(maxCapacity));

        Id = ++_counter;
        Name = name;
        this.MaxCapacity = maxCapacity;
    }
    
    public void AddSeat(int seatNumber, Seat seat)
    {
        ArgumentNullException.ThrowIfNull(seat);
        if (seatNumber <= 0)
            throw new ArgumentException("Seat number must be positive.", nameof(seatNumber));

        if (_seatsByNumber.ContainsKey(seatNumber))
            throw new InvalidOperationException(
                $"Seat number {seatNumber} already exists in hall {Name}.");

        if (_seatsByNumber.Count >= MaxCapacity)
            throw new InvalidOperationException(
                $"Hall {Name} is at full capacity ({MaxCapacity} seats).");

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