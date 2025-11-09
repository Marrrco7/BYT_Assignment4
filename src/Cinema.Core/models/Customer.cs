using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace Cinema.Core.models;

public class Customer : Person
{
    public string Email { get; private set; }
    public string HashPassword { get; private set; }
    public int BonusPoints { get; private set; }

    // public List<Order> Orders { get; private set; }
    // public List<review> reviews { get; private set; }

    public Customer(string firstName, string lastName, DateOnly dateOfBirth,
        string email, string rawPassword, int bonusPoints)
        : base(firstName, lastName, dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.");

        if (!EmailIsValid(email))
            throw new ArgumentException("Invalid email format.");

        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("Password cannot be empty.");

        if (rawPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.");

        if (bonusPoints < 0)
            throw new ArgumentException("Bonus points cannot be negative.");

        if (rawPassword.Length < 6)
            throw new ArgumentException("Raw password cannot be less than 6 characters.");

        Email = email;
        HashPassword = HashPasswordEncoder(rawPassword);
        BonusPoints = bonusPoints;
    }

    public void AddBonusPoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Cannot add negative bonus points.");
        BonusPoints += points;
    }

    public void RemoveBonusPoints(int points)
    {
        if (points < 0)
            throw new ArgumentException("Cannot remove negative bonus points.");

        if (points > BonusPoints)
            throw new ArgumentException("Not enough bonus points to remove.");

        BonusPoints -= points;
    }

    private string HashPasswordEncoder(string password)
    {
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }

    private bool EmailIsValid(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}