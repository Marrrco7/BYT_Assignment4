using Cinema.Core.models.operations;

namespace Cinema.Core.interfaces;

public interface ICleanerRole
{
    bool HasSafetyTraining { get; }
    DateOnly LastSafetyTrainingDate { get; }
    bool IsTrainingUpToDate();
    TimeSpan CalculateAverageCleaningTime(List<Shift> shifts);
}