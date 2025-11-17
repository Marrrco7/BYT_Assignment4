using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models;
using Cinema.Core.models.customers;


public class Customer : Person
{
    public static List<Customer> All { get; } = new();

    public string Email { get; private set; }
    public string HashPassword { get; private set; }

    [JsonIgnore]
    public List<Order> Orders { get; } = new();

    [JsonIgnore]
    public int BonusPoints => Orders.Sum(o => o.Points);


    public Customer(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string hashPassword)
        : base(firstName, lastName, dateOfBirth)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (!EmailIsValidStatic(email))
            throw new ArgumentException("Invalid email format.", nameof(email));

        if (string.IsNullOrWhiteSpace(hashPassword))
            throw new ArgumentException("Password hash cannot be empty.", nameof(hashPassword));

        Email = email;
        HashPassword = hashPassword;

        All.Add(this);
    }

    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        Orders.Add(order);
    }

    public void RemoveOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        Orders.Remove(order);
    }

    public int CheckBonusPoints() => BonusPoints;


    public static string HashRawPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));

        if (password.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(password));

        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    private static bool EmailIsValidStatic(string email)
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
    
    public static void SaveToFile(string filePath)
    {
        var json = JsonSerializer.Serialize(All, new JsonSerializerOptions
        {
            WriteIndented = true,
        });

        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);
        var customers = JsonSerializer.Deserialize<List<Customer>>(json);

        if (customers != null)
        {
            All.Clear();
            All.AddRange(customers);
        }
    }
}

