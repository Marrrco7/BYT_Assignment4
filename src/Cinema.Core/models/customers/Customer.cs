using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Cinema.Core.models.customers;
using Cinema.Core.models.sales;

namespace Cinema.Core.models;

public class Customer : Person
{
    public static List<Customer> All { get; } = new();
    public string Email { get; private set; }
    public string HashPassword { get; private set; }

    public int BonusPoints
    {
        get
        {
            return Orders.Sum(o => o.Points);
        }
    }
    public List<Order> Orders { get; private set; } = new();
    
    
    // public List<Review> Reviews { get; private set; } = new();

    public Customer(string firstName, string lastName, DateOnly dateOfBirth,
        string email, string rawPassword)
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

        Email = email;
        HashPassword = HashPasswordEncoder(rawPassword);

        All.Add(this);
    }


    public int CheckBonusPoints()
    {
        return BonusPoints;
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


    // public void AddReview(Review review)
    // {
    //     if (review == null)
    //         throw new ArgumentNullException(nameof(review));
    //
    //     Reviews.Add(review);
    // }

    // public void RemoveReview(Review review)
    // {
    //     if (review == null)
    //         throw new ArgumentNullException(nameof(review)); 
    //
    //     Reviews.Remove(review);
    // }
    
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