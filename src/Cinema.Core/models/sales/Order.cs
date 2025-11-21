using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.customers;
using Cinema.Core.models.roles;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.sales;

public enum OrderStatus
{
    Pending,
    Paid,
    Refunded
}

public enum TypeOfOrder
{
    Online,
    BoxOffice
}

public class Order
{
    private static readonly List<Order> _all = new();
    public static IReadOnlyList<Order> All => _all.AsReadOnly();

    private static int _counter = 0;

    public int Id { get; private set; }

    private DateTime _createdAt;
    public DateTime CreatedAt
    {
        get => _createdAt;
        private set
        {
            if (value > DateTime.Now)
                throw new ArgumentException("CreatedAt cannot be in the future.");
            _createdAt = value;
        }
    }
    private TypeOfOrder _typeOfOrder;
    public TypeOfOrder TypeOfOrder
    {
        get => _typeOfOrder;
        private set
        {
            _typeOfOrder = value;
        }
    }
    public OrderStatus Status { get; private set; }
    public string? EmailForBonusPoints { get; private set; }

    private List<Ticket> _tickets = new();
    public List<Ticket> Tickets
    {
        get => _tickets;                    
        set
        {
            if (value == null || value.Count == 0)
                throw new ArgumentException("Order must contain at least one ticket.");
            _tickets = value;
        }
    }


    // XOR
    private Customer? _customer;
    private Employee? _cashier;

    private Customer? Customer
    {
        get => _customer;
        set
        {
            _customer = value;
            ValidateXorRules();
        }
    }

    public Employee? Cashier
    {
        get => _cashier;
        set
        {
            if (value != null)
            {
                bool hasCashierRole = value.Roles.Any(r => r is CashierRole);
                if (!hasCashierRole)
                    throw new ArgumentException(
                        $"Employee {value.FirstName} {value.LastName} does not have CashierRole.");
            }

            _cashier = value;
            ValidateXorRules();
        }
    }

    public Order() { }

    public Order(
        DateTime createdAt,
        TypeOfOrder orderType,
        OrderStatus status,
        List<Ticket> tickets,
        Customer? customer = null,
        Employee? cashier = null,
        string? emailForBonusPoints = null)
    {
        Id = ++_counter;
        CreatedAt = createdAt;
        Tickets = tickets;
        EmailForBonusPoints = emailForBonusPoints;
        
        _customer = customer;
        _cashier = cashier;
        _typeOfOrder = orderType;

        if (customer != null) Customer = customer;
        if (cashier != null) Cashier = cashier;

        ValidateXorRules();

        Status = status;

        _all.Add(this);
    }

    public int CalculatePoints()
    {
        return Tickets.Count * 10;
    }
    
    private void ValidateXorRules()
    {
        if (_typeOfOrder == TypeOfOrder.Online)
        {
            if (_customer == null && _cashier != null)
                return;

            if (_customer == null) throw new ArgumentException("Online order must have a customer.");
            if (_cashier != null) throw new ArgumentException("Online order cannot have a cashier.");
        }
        else if (_typeOfOrder == TypeOfOrder.BoxOffice)
        {
            if (_cashier == null) throw new ArgumentException("Box office order must have a cashier.");
        }
    }
    
    public void ViewOrder()
    {
        Console.WriteLine("\n--- Order Details ---");
        Console.WriteLine("Order ID: " + Id);
        Console.WriteLine("Created: " + CreatedAt);
        Console.WriteLine("Type: " + TypeOfOrder);
        Console.WriteLine("Status: " + Status);
        Console.WriteLine("Tickets: " + Tickets.Count);
        Console.WriteLine("Points: " + CalculatePoints());

        if (TypeOfOrder == TypeOfOrder.Online)
        {
            Console.WriteLine("Customer: " + (Customer != null ? Customer.Email : "None"));
        }
        else
        {
            var cashierRole = Cashier?
                .Roles
                .FirstOrDefault(r => r is CashierRole) as CashierRole;

            var posLogin = cashierRole?.POSLogin ?? "None";

            Console.WriteLine("Cashier POS Login: " + posLogin);
            Console.WriteLine("Linked Customer Email: " + (EmailForBonusPoints ?? "None"));
        }
    }

    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot finalize order {Id}. Current status: {Status}");

        AssignTheOrderToCustomerByEmail();

        Status = OrderStatus.Paid;
        Console.WriteLine(
            $"Order {Id} finalized for {(Customer != null ? Customer.Email : "unlinked customer")}.");
    }

    public void RequestRefund()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException(
                $"Cannot refund order {Id}. Current status: {Status}");

        Status = OrderStatus.Refunded;
        Console.WriteLine($"Order {Id} refunded for {(Customer != null ? Customer.Email : "unlinked customer")}.");
    }

    private void AssignTheOrderToCustomerByEmail()
    {
        if (Customer != null || string.IsNullOrWhiteSpace(EmailForBonusPoints))
            return;

        Customer? matched = null;

        foreach (var c in Customer.All)
            if (c.Email != null &&
                c.Email.Equals(EmailForBonusPoints, StringComparison.OrdinalIgnoreCase))
            {
                matched = c;
                break;
            }

        if (matched != null)
        {
            Customer = matched;
            EmailForBonusPoints = matched.Email;
            matched.AddOrder(this);
            Console.WriteLine($"Order {Id} automatically linked to {matched.Email}");
        }
        else
        {
            Console.WriteLine($"No customer found with email {EmailForBonusPoints}");
        }
    }
    
    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        };

        var json = JsonSerializer.Serialize(All, options);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);
        var orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions
        {
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve
        });

        _all.Clear();
        if (orders != null && orders.Any())
        {
            _all.AddRange(orders);
            
            _counter = orders.Max(o => o.Id); 
        }
    }
}