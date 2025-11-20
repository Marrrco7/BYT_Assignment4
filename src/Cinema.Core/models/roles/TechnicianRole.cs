namespace Cinema.Core.models.roles;

public sealed class TechnicianRole : EmployeeRole
{
    private string _degree = null!;

    public string Degree
    {
        get => _degree;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Degree cannot be null, empty, or whitespace.", nameof(value));
            if (value.Length < 2)
                throw new ArgumentException("Degree name must be at least 2 characters long.", nameof(value));
            if (!value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-'))
                throw new ArgumentException("Degree can only contain letters, spaces, or hyphens.", nameof(value));

            _degree = value;
        }
    }

    public bool IsOnCall { get; }

    public TechnicianRole(string degree, bool isOnCall)
    {
        Degree = degree;
        IsOnCall = isOnCall;
    }
}
