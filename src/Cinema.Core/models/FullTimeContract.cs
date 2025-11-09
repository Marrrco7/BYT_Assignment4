using Cinema.Core.models;

namespace Cinema.Core.Models;

public sealed class FullTimeContract : EmploymentContract
{
    
    // add to the diagram 
    public static readonly decimal MIN_SALARY = 500m;
    public static readonly decimal MAX_SALARY = 3500m;

    public decimal Salary { get; }
    public bool HasBenefitsPlan { get; }

    public FullTimeContract(decimal salary, bool hasBenefitsPlan)
    {
        if (salary < MIN_SALARY)
            throw new ArgumentOutOfRangeException(nameof(salary),
                $"Salary must be at least {MIN_SALARY}.");
        
        if (salary > MAX_SALARY)
            throw new ArgumentOutOfRangeException(nameof(salary),
                $"Salary cannot exceed {MAX_SALARY}.");
        
        if (decimal.Round(salary, 2) != salary)
            throw new ArgumentException("Salary must have at most two decimal places.", nameof(salary));

        Salary = salary;
        HasBenefitsPlan = hasBenefitsPlan;
    }

}