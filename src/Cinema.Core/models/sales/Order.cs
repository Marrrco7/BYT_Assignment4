using Cinema.Core.models.customers;
using Cinema.Core.models.roles;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models;

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
    public static List<Order> All { get; } = new();

    private static int _counter = 0;

    public int Id { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public TypeOfOrder TypeOfOrder { get; private set; }
    public OrderStatus Status { get; private set; }
    private string? EmailForBonusPoints { get; set; }

    public  int Points => CalculatePoints();

    private List<Ticket> Tickets { get; set; }
    
    
    // XOR
    private Customer? Customer { get; set; }
    private Employee? Cashier { get; set; }


    public Order(
        DateTime createdAt,
        TypeOfOrder orderType,
        OrderStatus status,
        List<Ticket> tickets,
        Customer? customer = null,
        Employee? cashier = null,
        string? emailForBonusPoints = null)
    {
        if (createdAt > DateTime.Now)
            throw new ArgumentException("CreatedAt cannot be in the future.");
        if (tickets == null || tickets.Count == 0)
            throw new ArgumentException("Order must contain at least one ticket.");

        // XOR 
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

            bool hasCashierRole = cashier.Roles.Any(role => role is CashierRole);

            if (!hasCashierRole)
                throw new ArgumentException(
                    $"Employee {cashier.FirstName} {cashier.LastName} does not have CashierRole and cannot operate as cashier."
                );
        }

        Id = ++_counter;
        CreatedAt = createdAt;
        TypeOfOrder = orderType;
        Status = status;
        Tickets = tickets;
        Customer = customer;
        Cashier = cashier;
        EmailForBonusPoints = emailForBonusPoints;

        All.Add(this);

    }

    private int CalculatePoints()
    {
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
            var cashierRole = Cashier?
                .Roles
                .FirstOrDefault(r => r is CashierRole) as CashierRole;

            string posLogin = cashierRole?.POSLogin ?? "None";

            Console.WriteLine("Cashier POS Login: " + posLogin);
            Console.WriteLine("Linked Customer Email: " + (EmailForBonusPoints ?? "None"));
        }
    }

    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                $"Cannot finalize order {Id}. Current status: {Status}");
        
        AssingTheOrderToCustomerByEmail();

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
    
    private void AssingTheOrderToCustomerByEmail()
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
}
