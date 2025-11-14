using Cinema.Core.models.customers;
using Cinema.Core.models.roles;
using Cinema.Core.models.session;

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
    private static int _counter = 0;
    private int Id { get; set; }
    private DateTime CreatedAt { get; set; }
    private TypeOfOrder TypeOfOrder { get; set; }
    private OrderStatus Status { get; set; }
    private string? EmailForBonusPoints { get; set; }

    // XOR
    private Customer? Customer { get; set; }
    private CashierRole? Cashier { get; set; }

    private List<Ticket> Tickets { get; set; }
    private int Points => CalculatePoints();

    public Order(DateTime createdAt, TypeOfOrder orderType, OrderStatus status, List<Ticket> tickets,
        Customer? customer = null, CashierRole? cashier = null, string? emailForBonusPoints = null)
    {
        if (createdAt > DateTime.Now)
            throw new ArgumentException("CreatedAt cannot be in the future.");
        if (tickets == null || tickets.Count == 0)
            throw new ArgumentException("Order must contain at least one ticket.");

        // XOR validation
        if (orderType == TypeOfOrder.Online)
        {
            if (customer == null)
                throw new ArgumentException("Online order must have an associated customer.");
            if (cashier != null)
                throw new ArgumentException("Online order cannot have a cashier.");
        }
        else if (orderType == TypeOfOrder.BoxOffice)
        {
            if (cashier == null)
                throw new ArgumentException("Box office order must have an associated cashier.");
        }

        Id = ++_counter;
        CreatedAt = createdAt;
        TypeOfOrder = orderType;
        Status = status;
        Tickets = tickets;
        Customer = customer;
        Cashier = cashier;
        EmailForBonusPoints = emailForBonusPoints;

        Customer?.AddOrder(this);
    }

    private int CalculatePoints()
    {
        // based on price and amount of tickets?
        return Tickets.Count * 10; 
    }

    public void ViewOrder()
    {
        Console.WriteLine("\n--- Order Details ---");
        Console.WriteLine("Order ID: " + Id);
        Console.WriteLine("Created: " + CreatedAt);
        Console.WriteLine("Type: " + TypeOfOrder);
        Console.WriteLine("Status: " + Status);
        Console.WriteLine("Tickets: " + Tickets.Count);
        Console.WriteLine("Points: " + Points);

        if (TypeOfOrder == TypeOfOrder.Online)
        {
            Console.WriteLine("Customer: " + (Customer != null ? Customer.Email : "None"));
        }
        else
        {
            Console.WriteLine("Cashier: " + (Cashier != null ? Cashier.POSLogin : "None"));
            Console.WriteLine("Linked Customer Email: " + (EmailForBonusPoints ?? "None"));
        }
    }

    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException("Cannot finalize order " + Id + ". Current status: " + Status);
        
        TryLinkCustomerByEmail();

        Status = OrderStatus.Paid;
        Console.WriteLine("Order " + Id + " finalized for " + (Customer != null ? Customer.Email : "unlinked customer") + ".");
    }

    public void RequestRefund()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException("Cannot refund order " + Id + ". Current status: " + Status);

        Status = OrderStatus.Refunded;
        Console.WriteLine("Order " + Id + " refunded for " + (Customer != null ? Customer.Email : "unlinked customer") + ".");
    }
    
    private void TryLinkCustomerByEmail()
    {
        if (Customer != null || string.IsNullOrWhiteSpace(EmailForBonusPoints))
            return;

        Customer matched = null;

        foreach (var c in Customer.All)
        {
            if (c.Email != null && c.Email.ToLower() == EmailForBonusPoints.ToLower())
            {
                matched = c;
                break;
            }
        }

        if (matched != null)
        {
            Customer = matched;
            EmailForBonusPoints = matched.Email;
            matched.AddOrder(this);
            Console.WriteLine("Order " + Id + " automatically linked to " + matched.Email);
        }
        else
        {
            Console.WriteLine("No customer found with email " + EmailForBonusPoints);
        }
    }
}
