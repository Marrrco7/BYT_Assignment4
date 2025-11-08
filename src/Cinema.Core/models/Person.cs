namespace Cinema.Core.models;

public class Person
{
    
    private string FirstName { get; set; } 
    private string LastName { get; set; }
    private DateOnly DateOfBirth { get; set; }
    
    public  int Age 
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Now);

            int age = today.Year - DateOfBirth.Year;

            if (today.DayOfYear < DateOfBirth.DayOfYear)
                age--;

            return age;
        }
    }

    public Person(string firstName, string lastName, DateOnly dateOfBirth)
    {

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be null, empty or whitespace.");
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be null, empty or whitespace.");
        IsValidDateOfBirth(dateOfBirth);
        
        
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }


    private void IsValidDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);

        if (dateOfBirth > today)
            throw new ArgumentException("Date of birth cannot be in the future.");

        var minAllowedDate = today.AddYears(-120);

        if (dateOfBirth < minAllowedDate)
            throw new ArgumentException("Age must be between 0 and 120 years.");
    }

}