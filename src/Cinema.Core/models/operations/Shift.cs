using System.Text.Json;
using Cinema.Core.models.roles;
using Cinema.Core.models.sales;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.operations;

public class Shift
{
    private static readonly List<Shift> _all = new();
    public static IReadOnlyList<Shift> All => _all.AsReadOnly();
    
    private DateTime _startTime;
    public DateTime StartTime
    {
        get => _startTime;
        set
        {
            _startTime = value;
            ValidateTimes();
        }
    }

    private DateTime _endTime;
    public DateTime EndTime
    {
        get => _endTime;
        private set
        {
            _endTime = value;
            ValidateTimes();
        }
    }
    private CleanerRole _cleaner;
    public CleanerRole Cleaner { get => _cleaner; private set => _cleaner = value; }
    
    private Hall _hall;

    public Hall Hall
    {
        get => _hall;
        private set => _hall = value;
    }

    public bool IsDeleted { get; private set; }
    
    // Constructors
    public Shift(DateTime startTime, DateTime endTime, CleanerRole cleaner, Hall hall)
    {
        StartTime = startTime;
        EndTime = endTime;
        Cleaner = cleaner;
        
        ValidateTimes();
        
        // reverse connection
        Cleaner.AddShiftInternal(this);
        // SetCleaner(cleaner);
        SetHall(hall);
        
        _all.Add(this);
    }
    
    public void SetHall(Hall newHall)
    {
        if (newHall == null) throw new ArgumentNullException(nameof(newHall));

        if (_hall == newHall) return;

        // disconnect old
        if (_hall != null)
        {
            _hall.RemoveShift(this);
        }

        // connect new
        _hall = newHall;
        _hall.AddShift(this);
    }
    
    /*public void SetCleaner(CleanerRole newCleaner)
    {
        if (newCleaner == null) throw new ArgumentNullException(nameof(newCleaner));
        
        if (_cleaner == newCleaner) return;

        // disconnect old (Assuming CleanerRole has RemoveShift)
        if (_cleaner != null)
        {
            _cleaner.RemoveShift(this); 
        }

        // connect new
        _cleaner = newCleaner;
        
        // propagate (Assuming CleanerRole has AddShift)
        _cleaner.AddShift(this);
    }*/
    
    // Validator
    private void ValidateTimes()
    {
        if (_endTime <= _startTime)
            throw new ArgumentException("Shift end time must be after start time.");

        if ((_endTime - _startTime).TotalHours > 4)
            throw new ArgumentException("Shift duration cannot exceed 4 hours.");
    }
    
    // Serialization
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
        var shifts = JsonSerializer.Deserialize<List<Shift>>(json);

        _all.Clear();
        if (shifts != null)
        {
            _all.AddRange(shifts);
        }
    }

    // Business logic
    public TimeSpan CalculateDuration()
    {
        return EndTime - StartTime;
    }
    
    public void EditShift(DateTime newStartTime, DateTime newEndTime)
    {
        StartTime = newStartTime;
        EndTime = newEndTime;
    }
    
    public void DeleteShift()
    {
        IsDeleted = true;
    }
}