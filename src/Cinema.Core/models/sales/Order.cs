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
    // Static extent

    private static readonly List<Order> _all = new();
    public static IReadOnlyList<Order> All => _all.AsReadOnly();

    private static int _counter = 0;

    // Fields

    private DateTime _createdAt;
    private TypeOfOrder _typeOfOrder;
    
    [JsonIgnore]
    private readonly List<Ticket> _tickets = new();

    // XOR
    private Customer? _customer;
    private Employee? _cashier;

    // Properties

    public int Id { get; private set; }

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

    public TypeOfOrder TypeOfOrder
    {
        get => _typeOfOrder;
        private set => _typeOfOrder = value;
    }

    public OrderStatus Status { get; private set; }

    public string? EmailForBonusPoints { get; private set; }

    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

    public Customer? Customer => _customer;

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

    // Constructors

    public Order()
    {
    }

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
        EmailForBonusPoints = emailForBonusPoints;

        TypeOfOrder = orderType;
        
        SetTickets(tickets);

        if (customer != null)
            SetCustomer(customer);

        if (cashier != null)
            Cashier = cashier;

        ValidateXorRules();

        Status = status;

        _all.Add(this);
    }
    

    // Associations: Customer (0..1) – reverse connection

    public void SetCustomer(Customer? customer)
    {
        if (_customer == customer)
            return;

        if (_customer != null)
        {
            var oldCustomer = _customer;
            _customer = null;
            oldCustomer.RemoveOrderInternal(this);
        }

        if (customer != null)
        {
            _customer = customer;
            customer.AddOrderInternal(this);
        }
        else
        {
            _customer = null;
        }

        ValidateXorRules();
    }

    internal void SetCustomerInternal(Customer? customer)
    {
        _customer = customer;
        ValidateXorRules();
    }

    // Business logic

    public int CalculatePoints()
    {
        return Tickets.Count * 10;
    }

    private void ValidateXorRules()
    {
        if (TypeOfOrder == TypeOfOrder.Online)
        {
            if (_customer == null && _cashier != null)
                return;

            if (_customer == null)
                throw new ArgumentException("Online order must have a customer.");
            if (_cashier != null)
                throw new ArgumentException("Online order cannot have a cashier.");
        }
        else if (TypeOfOrder == TypeOfOrder.BoxOffice)
        {
            if (_cashier == null)
                throw new ArgumentException("Box office order must have a cashier.");
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
        Console.WriteLine(
            $"Order {Id} refunded for {(Customer != null ? Customer.Email : "unlinked customer")}.");
    }

    private void AssignTheOrderToCustomerByEmail()
    {
        if (Customer != null || string.IsNullOrWhiteSpace(EmailForBonusPoints))
            return;

        Customer? matched = null;

        foreach (var c in Customer.All)
        {
            if (c.Email != null &&
                c.Email.Equals(EmailForBonusPoints, StringComparison.OrdinalIgnoreCase))
            {
                matched = c;
                break;
            }
        }

        if (matched != null)
        {
            SetCustomer(matched);
            EmailForBonusPoints = matched.Email;
            Console.WriteLine($"Order {Id} automatically linked to {matched.Email}");
        }
        else
        {
            Console.WriteLine($"No customer found with email {EmailForBonusPoints}");
        }
    }

    // Persistence

    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            ReferenceHandler = ReferenceHandler.Preserve
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
            ReferenceHandler = ReferenceHandler.Preserve
        });

        _all.Clear();
        if (orders != null && orders.Any())
        {
            _all.AddRange(orders);
            _counter = orders.Max(o => o.Id);
        }
    }
    
    // Composition | Ticket
    
    private void SetTickets(IEnumerable<Ticket> tickets)
    {
        var ticketList = tickets?.ToList() ?? new List<Ticket>();
        
        if (ticketList.Count == 0)
            throw new ArgumentException("Order must contain at least one ticket.");

        foreach (var ticket in ticketList)
        {
            if (ticket.Order != null && ticket.Order != this)
                throw new InvalidOperationException("Ticket is already part of another Order (Composition rule: Part cannot be shared).");
            
            if (!_tickets.Contains(ticket))
            {
                _tickets.Add(ticket);
                ticket.Order = this;
            }
        }
    }
    
    internal void RemoveTicketInternal(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));
        
        _tickets.Remove(ticket);
    }

    //delete method for the Whole
    public static bool DeleteOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));
        
        if (order._tickets.Any())
        {
            foreach (var ticket in order._tickets.ToList())
            {
                Ticket.DeleteOrderPart(ticket);
            }
        }
        
        return _all.Remove(order);
    }
}
