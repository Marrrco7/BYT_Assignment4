using Cinema.Core.Models;

namespace Cinema.Core.models;

public class Shift
{
    private DateTime StartTime { get; set; }
    private DateTime EndTime { get; set; }
    private TimeSpan Duration => EndTime - StartTime;
    
    public CleanerRole Cleaner { get; private set; }
    public Hall Hall { get; private set; }
    
    public Shift(DateTime startTime, DateTime endTime, CleanerRole cleaner, Hall hall)
    {
        if (endTime <= startTime)
        {
            throw new ArgumentException("Shift end time must be after start time.");
        }
        
        if ((endTime - startTime).TotalHours > 4)
        {
            throw new ArgumentException("Shift duration cannot exceed 4 hours.");
        }

        StartTime = startTime;
        EndTime = endTime;
        Cleaner = cleaner;
        Hall = hall;
    }
    
    public override string ToString()
    {
        return $"[{StartTime:dd MMMM yyyy}] Cleaner worked in hall from {StartTime:HH:mm} to {EndTime:HH:mm} ({Duration.TotalMinutes:F0} min)";
    }
}