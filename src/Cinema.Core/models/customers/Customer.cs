using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.customers;
using Cinema.Core.models.sales;

public class Customer : Person
{
    private static readonly List<Customer> _all = new();
    public static IReadOnlyList<Customer> All => _all.AsReadOnly();

    private string _email;
    private string _hashPassword;

    public string Email
    {
        get => _email;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Email cannot be empty.", nameof(value));

            if (!EmailIsValidStatic(value))
                throw new ArgumentException("Invalid email format.", nameof(value));

            _email = value;
        }
    }

    public string HashPassword
    {
        get => _hashPassword;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Password hash cannot be empty.", nameof(value));

            _hashPassword = value;   
        }
    }

    [JsonIgnore]
    private readonly List<Order> _orders = new();

    [JsonIgnore]
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    public Customer(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        string email,
        string rawPassword)
        : base(firstName, lastName, dateOfBirth)
    {
        Email = email;

        
        if (string.IsNullOrWhiteSpace(rawPassword))
            throw new ArgumentException("Password cannot be empty.", nameof(rawPassword));

        if (rawPassword.Length < 6)
            throw new ArgumentException("Password must be at least 6 characters long.", nameof(rawPassword));

        var hash = HashedRawPassword(rawPassword); 
        HashPassword = hash;                     

        _all.Add(this);
    }


    
    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        _orders.Add(order);
    }

    public void RemoveOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        _orders.Remove(order);
    }


    
    private string HashedRawPassword(string password)
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
        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(_all, options);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);
        var customers = JsonSerializer.Deserialize<List<Customer>>(json);

        _all.Clear();
        if (customers != null)
        {
            _all.AddRange(customers);
        }
    }

    public int GetBonusPoints()
    {
        return _orders.Sum(o => o.CalculatePoints());
    }
}
