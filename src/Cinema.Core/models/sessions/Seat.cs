namespace Cinema.Core.models.sessions
{
    public enum SeatType
    {
        Normal,
        Vip
    }
    
    public class Seat
    { 
        public static List<Seat> All { get; } = new();
        public SeatType Type { get; private set; }
        public decimal NormalPrice { get; private set; }
        public bool IsAccessible { get; private set; }
        public decimal TicketMultiplier { get; private set; } = 1.8m;

        public Seat(
            SeatType type,
            decimal normalPrice,
            bool isAccessible,
            decimal? ticketMultiplier = null)
        {
            if (normalPrice < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(normalPrice));
            
            Type = type;
            NormalPrice = normalPrice;
            IsAccessible = isAccessible;

            if (ticketMultiplier.HasValue)
                TicketMultiplier = ticketMultiplier.Value;

            All.Add(this);
        }

        public decimal CalculateFinalSeatPrice()
        {
            if (Type == SeatType.Vip)
                return NormalPrice * TicketMultiplier;

            return NormalPrice;
        }
    }
}