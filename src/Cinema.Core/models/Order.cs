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
    private OrderStatus OrderStatus { get; set; }
    private int Points { get; set; }
    
    private Customer Customer { get; set; }
    
    // private Cashier Cashier { get; set; }
    
    // private List<Ticket> Tickets { get; set; }
    
    public Order(int id, DateTime createdAt, OrderStatus status, int points, TypeOfOrder typeOfOrder)
    {
        IsValidCreatedAt(createdAt); 
        
        Id = id;
        CreatedAt = createdAt;
        OrderStatus = status;
        Points = points;
        TypeOfOrder = typeOfOrder;
    }

    public void ViewOrder()
    {
        Console.WriteLine("\n--- Order Details ---");
        Console.WriteLine("Order: " + Id);
        Console.WriteLine("Status: " + OrderStatus); 
        Console.WriteLine("Created: " + CreatedAt);
        Console.WriteLine("Type: " + TypeOfOrder);
        
        if (Customer != null)
        {
            Console.WriteLine("Customer: " + Customer);
        }
        else
        {
            Console.WriteLine("Customer: Unknown");
        }
        
        // Console.WriteLine("Cashier: " + Cashier.POSLogin);
        // Console.WriteLine("Ticket: " + Ticket);
    }
    
    public static List<Order> ListOfOrdersMadeByCustomer(Customer customer)
    {
        return new List<Order>();
    }
    
    private static void IsValidCreatedAt(DateTime createdAt)
    {
        if (createdAt > DateTime.Now) 
            throw new ArgumentException("CreatedAt cannot be in the future.");
    }

    public void RefundOrder()
    {
        if (this.OrderStatus == OrderStatus.Paid)
        {
            this.OrderStatus = OrderStatus.Refunded;
            Console.WriteLine("Order " + this.Id + " has been refunded.");
        }
        else
        {
            Console.WriteLine("Cannot refund order " + this.Id + ". Status is: " + this.OrderStatus);
        }
    }

    public void FinalizeOrder()
    {
        if (this.OrderStatus == OrderStatus.Pending)
        {
            this.OrderStatus = OrderStatus.Paid;
            Console.WriteLine("Order " + this.Id + " has been finalized.");
        }
        else
        {
            Console.WriteLine("Cannot finalize order " + this.Id + ". Status is: " + this.OrderStatus);
        }
    }
    
}