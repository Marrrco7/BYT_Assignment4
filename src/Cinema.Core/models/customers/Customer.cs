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

    public string Email { get; set; }
    public string HashPassword { get; set; }

    [JsonIgnore]
    public List<Order> Orders { get; private set; } = new();

    [JsonIgnore]
    public int BonusPoints => Orders.Sum(o => o.Points);

    public  Customer(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string hashPassword)
        : base(firstName, lastName, dateOfBirth)
    {
        Email = email;
        HashPassword = hashPassword;

        All.Add(this);
    }

    public static Customer Create(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string rawPassword)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty.", nameof(email));

        if (!EmailIsValidStatic(email))
            throw new ArgumentException("Invalid email format.", nameof(email));

        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(rawPassword));

        if (rawPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(rawPassword));

        var hash = HashPasswordEncoderStatic(rawPassword);

        return new Customer(firstName, lastName, dateOfBirth, email, hash);
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


    private static string HashPasswordEncoderStatic(string password)
    {
        using var sha256 = SHA256.Create();
        byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Serialize(All, options);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var customers = JsonSerializer.Deserialize<List<Customer>>(json, options);

        if (customers != null)
        {
            All.Clear();
            All.AddRange(customers);
        }
    }
}
