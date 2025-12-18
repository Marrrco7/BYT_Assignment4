using System;
using System.Collections.Generic;
using Cinema.Core.models.operations;
using System.Text.Json.Serialization;
using Cinema.Core.interfaces;
using Cinema.Core.models.customers; // Added

namespace Cinema.Core.models.roles;

public sealed class CleanerRole : ICleanerRole
{
    private DateOnly _lastSafetyTrainingDate;
    public bool HasSafetyTraining { get; private set; }
    [JsonIgnore]
    private readonly List<Shift> _shifts = new();
    [JsonIgnore]
    public IReadOnlyList<Shift> Shifts => _shifts.AsReadOnly();

    public DateOnly LastSafetyTrainingDate
    {
        get => _lastSafetyTrainingDate;
        private set => _lastSafetyTrainingDate = value;
    }
    
    private Employee _employee;
    public Employee Employee => _employee;

    // Constructors
    public CleanerRole(Employee employee, bool hasTraining, DateOnly lastTraining)
    {
        _employee = employee ?? throw new ArgumentNullException(nameof(employee));

        HasSafetyTraining = hasTraining;
        LastSafetyTrainingDate = lastTraining;

        employee.AddCleanerRole(this);
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
            totalMinutes += shift.CalculateDuration().TotalMinutes;

        var avgMinutes = totalMinutes / shifts.Count;

        return TimeSpan.FromMinutes(avgMinutes);
    }
    
    // Shift
    public void AddShift(Shift shift)
    {
        if (shift == null) 
            throw new ArgumentNullException(nameof(shift), "Shift cannot be null.");

        // stop infinite loop
        if (_shifts.Contains(shift)) return;

        _shifts.Add(shift);
        
        // tell the shift to point to this cleaner
        if (shift.Cleaner != this)
        {
            shift.SetCleaner(this);
        }
    }
    
    public void RemoveShift(Shift shift)
    {
        if (shift == null)
            throw new ArgumentNullException(nameof(shift), "Shift cannot be null.");
        
        if (!_shifts.Contains(shift)) return;

        _shifts.Remove(shift);
    }
    
    // Composition
    public void DeletePart()
    {
        _employee.RemoveCleanerRole(this);
        _employee = null!;
    }
}