using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions
{
    public class Ticket
    {
        // Fields
        public static List<Ticket> All { get; } = new();
        public Session Session { get; }
        public Seat Seat { get; }
        public bool IsBooked { get; private set; }
        
        // Composition
        // it's basically reference to the whole (Order). internal set allows Order to link it
        public Order Order { get; internal set; }

        public Ticket(Session session, Seat seat, Order order)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Seat = seat ?? throw new ArgumentNullException(nameof(seat));
            Order = order ?? throw new ArgumentNullException(nameof(order));

            All.Add(this);
        }
        
        // Business logic

        public decimal CalculateFinalPrice(decimal bonusPointsUsed = 0)
        {
            decimal price = Seat.CalculateFinalSeatPrice();

            Promotion? activePromo = Promotion.All
                .FirstOrDefault(p => p.IsActive());

            if (activePromo != null)
            {
                price -= activePromo.DiscountValue; 
            }

            if (bonusPointsUsed > 0)
            {
                price -= bonusPointsUsed;
            }

            return price < 0 ? 0 : price;
        }

        public void BookTicket()
        {
            if (IsBooked)
                throw new InvalidOperationException("Ticket is already booked.");

            IsBooked = true;
        }
        
        // Order    
        
        internal static void DeletePart(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));
            
            if (!All.Contains(ticket))
                return;
            
            ticket.Order.RemoveTicketInternal(ticket); 
            
            All.Remove(ticket); 
        }
    }
}