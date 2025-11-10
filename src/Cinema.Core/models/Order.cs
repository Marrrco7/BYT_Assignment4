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
    private int Id { get; set; }
    private DateTime CreatedAt { get; set; }
    private TypeOfOrder TypeOfOrder { get; set; }
    private OrderStatus Status { get; set; }
    public int? BonusPoints { get; private set; }

    // XOR
    private Customer? Customer { get; set; }
    private CashierRole? Cashier { get; set; }

    private List<Ticket> Tickets { get; set; }
    private int Points => CalculatePoints();

    public Order(int id, DateTime createdAt, TypeOfOrder type, OrderStatus status, List<Ticket> tickets,
        Customer? customer = null, CashierRole? cashier = null, int? bonusPoints = null)
    {
        if (createdAt > DateTime.Now)
            throw new ArgumentException("CreatedAt cannot be in the future.");

        if (tickets == null || tickets.Count == 0)
            throw new ArgumentException("Order must contain at least one ticket.");

        // XOR
        if (type == TypeOfOrder.Online)
        {
            if (customer == null)
                throw new ArgumentException("Online order must have an associated customer.");
            if (cashier != null)
                throw new ArgumentException("Online order cannot have a cashier.");
        }
        else if (type == TypeOfOrder.BoxOffice)
        {
            if (cashier == null)
                throw new ArgumentException("Box office order must have an associated cashier.");
            
            // customer is not required for BoxOffice
        }

        Id = id;
        CreatedAt = createdAt;
        TypeOfOrder = type;
        Status = status;
        Tickets = tickets;
        Customer = customer;
        Cashier = cashier;
        BonusPoints = bonusPoints;
    }

    private int CalculatePoints()
    {
        // based on price and amount of tickets?
        return Tickets.Count * 10; 
    }

    public void ViewOrder()
    {
        Console.WriteLine("\n--- Order Details ---");
        Console.WriteLine($"Order ID: {Id}");
        Console.WriteLine($"Created: {CreatedAt}");
        Console.WriteLine($"Type: {TypeOfOrder}");
        Console.WriteLine($"Status: {Status}");
        Console.WriteLine($"Points: {Points}");
        Console.WriteLine($"Tickets: {Tickets.Count}");

        if (TypeOfOrder == TypeOfOrder.Online)
        {
            Console.WriteLine($"Customer: {Customer?.Email}");
        }
        else
        {
            Console.WriteLine($"Cashier: {Cashier.POSLogin}");
            Console.WriteLine($"Linked Customer: {(Customer != null ? Customer.Email : "None")}");
        }
    }

    public void FinalizeOrder()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot finalize order {Id}. Current status: {Status}");

        Status = OrderStatus.Paid;
        
        if (Customer != null)
        {
            Customer.AddBonusPoints(Points);
            Console.WriteLine($"Order {Id} finalized. {Points} bonus points added to customer.");
        }
        else
        {
            Console.WriteLine($"Order {Id} finalized (no linked customer).");
        }
    }

    public void RequestRefund()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot refund order {Id}. Current status: {Status}");

        Status = OrderStatus.Refunded;

        if (Customer != null)
        {
            Customer.RemoveBonusPoints(Points);
            Console.WriteLine($"Order {Id} refunded. {Points} bonus points removed from customer.");
        }
        else
        {
            Console.WriteLine($"Order {Id} refunded (no linked customer).");
        }
    }
}
