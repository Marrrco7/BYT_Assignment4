namespace Cinema.Core.models.customers;

public abstract class Person
{
    
    public string FirstName { get; private set; } 
    public string LastName { get; private set; }
    public DateOnly DateOfBirth { get; private set; }
    
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
        ValidDateOfBirth(dateOfBirth);
        
        
        FirstName = firstName;
        LastName = lastName;
        DateOfBirth = dateOfBirth;
    }


    private void ValidDateOfBirth(DateOnly dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);

        if (dateOfBirth > today)
            throw new ArgumentException("Date of birth cannot be in the future.");

        var minAllowedDate = today.AddYears(-120);

        if (dateOfBirth < minAllowedDate)
            throw new ArgumentException("Age must be between 0 and 120 years.");
    }

}