using Cinema.Core.models;

namespace Cinema.Core.Models;

public sealed class TechnicianRole : EmployeeRole
{
    public string Degree { get; }
    public bool IsOnCall { get; }

    public TechnicianRole(string degree, bool isOnCall)
    {
        if (string.IsNullOrWhiteSpace(degree))
            throw new ArgumentException("Degree cannot be null, empty, or whitespace.", nameof(degree));
        if (degree.Length < 2)
            throw new ArgumentException("Degree name must be at least 2 characters long.", nameof(degree));
        if (!degree.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-'))
            throw new ArgumentException("Degree can only contain letters, spaces, or hyphens.", nameof(degree));

        Degree = degree;
        IsOnCall = isOnCall;
    }
}