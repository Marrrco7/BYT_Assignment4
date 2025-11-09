using Cinema.Core.models;

namespace Cinema.Core.Models;

public sealed class PartTimeContract : EmploymentContract
{
    public static readonly decimal MIN_HOURLY_RATE = 5.00m;
    public static readonly decimal MAX_HOURLY_RATE = 50.00m;
    public static readonly int MIN_WEEKLY_HOURS = 1;
    public static readonly int MAX_WEEKLY_HOURS = 30;

    public decimal HourlyRate { get; }
    public int MaxWeekHours { get; }

    public PartTimeContract(decimal hourlyRate, int maxWeekHours)
    {
        if (hourlyRate < MIN_HOURLY_RATE || hourlyRate > MAX_HOURLY_RATE)
            throw new ArgumentOutOfRangeException(nameof(hourlyRate),
                $"Hourly rate must be between {MIN_HOURLY_RATE} and {MAX_HOURLY_RATE}.");

        if (maxWeekHours < MIN_WEEKLY_HOURS || maxWeekHours > MAX_WEEKLY_HOURS)
            throw new ArgumentOutOfRangeException(nameof(maxWeekHours),
                $"Weekly hours must be between {MIN_WEEKLY_HOURS} and {MAX_WEEKLY_HOURS}.");

        HourlyRate = hourlyRate;
        MaxWeekHours = maxWeekHours;
    }

}