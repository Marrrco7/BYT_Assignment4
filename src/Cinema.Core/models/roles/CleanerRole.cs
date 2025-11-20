using System;
using System.Collections.Generic;
using Cinema.Core.models.operations;

namespace Cinema.Core.models.roles;

public sealed class CleanerRole : EmployeeRole
{
    private DateOnly _lastSafetyTrainingDate;

    public bool HasSafetyTraining { get; private set; }

    public DateOnly LastSafetyTrainingDate
    {
        get => _lastSafetyTrainingDate;
        private set => _lastSafetyTrainingDate = value;
    }

    public TimeSpan AvgCleaningTime { get; private set; }

    public CleanerRole(bool hasSafetyTraining, DateOnly lastSafetyTrainingDate)
    {
        HasSafetyTraining = hasSafetyTraining;
        LastSafetyTrainingDate = lastSafetyTrainingDate;
        AvgCleaningTime = TimeSpan.Zero;
    }

    public bool IsTrainingUpToDate()
    {
        if (!HasSafetyTraining)
            return false;

        var sixMonthsAgo = DateOnly.FromDateTime(DateTime.Now).AddMonths(-6);
        return LastSafetyTrainingDate > sixMonthsAgo;
    }

    public TimeSpan CalculateAverageCleaningTime(List<Shift> shifts)
    {
        if (shifts == null || shifts.Count == 0)
            throw new ArgumentException("Shift list cannot be null or empty.", nameof(shifts));

        double totalMinutes = 0;

        foreach (var shift in shifts)
            totalMinutes += shift.Duration.TotalMinutes;

        var avgMinutes = totalMinutes / shifts.Count;
        AvgCleaningTime = TimeSpan.FromMinutes(avgMinutes);

        return AvgCleaningTime;
    }
}
