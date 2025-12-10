namespace Cinema.Core.models.customers;

using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.sales;

public class Customer : Person
{
    // Static extent

    private static readonly List<Customer> _all = new();
    public static IReadOnlyList<Customer> All => _all.AsReadOnly();

    // Fields

    private string _email;
    private string _hashPassword;

    [JsonIgnore]
    private readonly List<Order> _orders = new();

    [JsonIgnore]
    private readonly List<Review> _reviews = new();

    // Properties

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
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<Review> Reviews => _reviews.AsReadOnly();

    // Constructors

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

        _hashPassword = HashedRawPassword(rawPassword);

        _all.Add(this);
    }

    [JsonConstructor]
    public Customer(
        string firstName,
        string lastName,
        string email,
        DateOnly dateOfBirth,
        string hashPassword)
        : base(firstName, lastName, dateOfBirth)
    {
        _email = email;
        _hashPassword = hashPassword;
    }

    // Associations: Orders (reverse connection без internal)

    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (_orders.Contains(order))
        {
            if (order.Customer != this)
            {
                order.SetCustomer(this);
            }
            return;
        }

        _orders.Add(order);

        if (order.Customer != this)
        {
            order.SetCustomer(this);
        }
    }

    public void RemoveOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (!_orders.Contains(order))
            return;

        _orders.Remove(order);

        if (order.Customer == this)
        {
            order.SetCustomer(null);
        }
    }

    // Associations: Reviews (Customer–Session via Review, тоже без internal)

    public void AddReview(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (_reviews.Contains(review))
        {
            if (review.Author != this)
            {
                review.SetAuthor(this);
            }
            return;
        }

        _reviews.Add(review);

        if (review.Author != this)
        {
            review.SetAuthor(this);
        }
    }

    public void RemoveReview(Review review)
    {
        if (review == null)
            throw new ArgumentNullException(nameof(review));

        if (!_reviews.Contains(review))
            return;

        _reviews.Remove(review);

        if (review.Author == this)
        {
            review.SetAuthor(null);
        }
    }

    // Business logic

    public int GetBonusPoints()
    {
        return _orders.Sum(o => o.CalculatePoints());
    }

    // Persistence

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

    // Private helpers

    private string HashedRawPassword(string password)
    {
        using var sha256 = SHA256.Create();
        return Convert.ToBase64String(sha256.ComputeHash(Encoding.UTF8.GetBytes(password)));
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
}
