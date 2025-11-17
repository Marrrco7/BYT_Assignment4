using Cinema.Core.models.operations;

namespace Cinema.Core.models.roles;

public sealed class CleanerRole : EmployeeRole
{
    public bool HasSafetyTraining { get; }
    
    
    // add to the diagram 
    public DateOnly LastSafetyTrainingDate { get; }
    
    public TimeSpan AvgCleaningTime { get; private set; }

    public CleanerRole(
        bool hasSafetyTraining,
        DateOnly lastSafetyTrainingDate)
    {
        HasSafetyTraining = hasSafetyTraining;
        LastSafetyTrainingDate = lastSafetyTrainingDate;
        AvgCleaningTime = TimeSpan.Zero;
    }

    
    // if the last training date > 6m -> error (return false )
    public bool IsTrainingUpToDate()
    {
        if (!HasSafetyTraining)
            return false;

        var sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now).AddMonths(-6);
        return LastSafetyTrainingDate > sixMonthsAgo;
    }

    // calculating avg time of cleaning taking values
    // from Shift (end - start )
    //    DateAndTime - startTime and EndTime (note for the Shift class )

    public TimeSpan CalculateAverageCleaningTime(List<Shift> shifts)
    {
        if (shifts == null || shifts.Count == 0)
            throw new ArgumentException("Shift list cannot be null or empty.", nameof(shifts));
    
        double totalMinutes = 0;
    
        foreach (var shift in shifts)
        {
            totalMinutes += shift.Duration.TotalMinutes;
        }
    
        var avgMinutes = totalMinutes / shifts.Count;
        AvgCleaningTime = TimeSpan.FromMinutes(avgMinutes);
    
        return AvgCleaningTime;
    }
}