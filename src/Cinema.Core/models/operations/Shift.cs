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
        set
        {
            _endTime = value;
            ValidateTimes();
        }
    }

    public TimeSpan Duration => EndTime - StartTime;
    
    public CleanerRole Cleaner { get; private set; }
    public Hall Hall { get; private set; }
    
    public Shift(DateTime startTime, DateTime endTime, CleanerRole cleaner, Hall hall)
    {
        StartTime = startTime;
        EndTime = endTime;
        Cleaner = cleaner;
        Hall = hall;
        
        ValidateTimes();
        
        _all.Add(this);
    }
    
    public override string ToString()
    {
        return $"[{StartTime:dd MMMM yyyy}] Cleaner worked in hall from {StartTime:HH:mm} to {EndTime:HH:mm} ({Duration.TotalMinutes:F0} min)";
    }
    
    private void ValidateTimes()
    {
        if (_endTime <= _startTime)
            throw new ArgumentException("Shift end time must be after start time.");

        if ((_endTime - _startTime).TotalHours > 4)
            throw new ArgumentException("Shift duration cannot exceed 4 hours.");
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
        var shifts = JsonSerializer.Deserialize<List<Shift>>(json);

        _all.Clear();
        if (shifts != null)
        {
            _all.AddRange(shifts);
        }
    }
}