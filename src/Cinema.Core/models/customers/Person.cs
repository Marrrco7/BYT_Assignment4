namespace Cinema.Core.models.customers;

public abstract class Person
{
    private string _firstName;
    private string _lastName;
    private DateOnly _dateOfBirth;

    public string FirstName
    {
        get => _firstName;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("First name cannot be null, empty or whitespace.", nameof(value));

            _firstName = value;
        }
    }

    public string LastName
    {
        get => _lastName;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Last name cannot be null, empty or whitespace.", nameof(value));

            _lastName = value;
        }
    }

    public DateOnly DateOfBirth
    {
        get => _dateOfBirth;
        private set
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            if (value > today)
                throw new ArgumentException("Date of birth cannot be in the future.", nameof(value));

            var minAllowedDate = today.AddYears(-120);
            if (value < minAllowedDate)
                throw new ArgumentException("Age must be between 0 and 120 years.", nameof(value));

            _dateOfBirth = value;
        }
    }

    public int Age
    {
        get
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            int age = today.Year - DateOfBirth.Year;
            if (today.DayOfYear < DateOfBirth.DayOfYear)
                age--;

            return age;
        }
    }

    protected Person(string firstName, string lastName, DateOnly dateOfBirth)
    {
        FirstName   = firstName;     
        LastName    = lastName;      
        DateOfBirth = dateOfBirth;   
    }
}