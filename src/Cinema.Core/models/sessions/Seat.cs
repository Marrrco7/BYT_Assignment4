using System.Text.Json.Serialization;

namespace Cinema.Core.models.sessions
{
    public enum SeatType
    {
        Normal,
        Vip
    }
    
    public class Seat
    { 
        private static int _nextId = 1;
        public int Id { get; }

        // ====== Fields ======
        public static List<Seat> All { get; } = new();
        public SeatType Type { get; private set; }
        
        private decimal _price;

        public decimal NormalPrice
        {
            get => _price;
            private set
            {
                if (value < 0)
                    throw new ArgumentException("Price cannot be negative.", nameof(value));
                
                _price = value;
            }
        }

        public bool IsAccessible { get; private set; }
        public decimal TicketMultiplier { get; private set; } = 1.8m;
        
        [JsonIgnore]
        private readonly List<Ticket> _tickets = new();
        
        [JsonIgnore]
        public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

        public Seat(SeatType type, decimal normalPrice, bool isAccessible, decimal? ticketMultiplier = null)
        {
            Id = _nextId++;         

            Type = type;
            NormalPrice = normalPrice;
            IsAccessible = isAccessible;

            if (ticketMultiplier.HasValue)
                TicketMultiplier = ticketMultiplier.Value;

            All.Add(this);
        }

        // ===== Business logic =====
        public decimal CalculateFinalSeatPrice()
        {
            if (Type == SeatType.Vip)
                return NormalPrice * TicketMultiplier;

            return NormalPrice;
        }
        
        // ===== Ticket 
        public void AddTicket(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));
            
            if (ticket.Seat != this)
                throw new InvalidOperationException("Ticket must be linked to this Seat instance.");

            if (!_tickets.Contains(ticket))
                _tickets.Add(ticket);
        }
        
        public void RemoveTicket(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            _tickets.Remove(ticket);
        }
    }
}
