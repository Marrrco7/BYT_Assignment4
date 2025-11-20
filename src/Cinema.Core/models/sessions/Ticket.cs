namespace Cinema.Core.models.sessions
{
    public class Ticket
    {
        public static List<Ticket> All { get; } = new();
        public Session Session { get; }
        public Seat Seat { get; }
        public decimal FinalPrice => CalculateFinalPrice();
        public bool IsBooked { get; private set; }
        public decimal DiscountAmount { get; private set; }   
        public int BonusPointsUsed { get; private set; }     

        public Ticket(
            Session session,
            Seat seat,
            decimal discountAmount = 0m,
            int bonusPointsUsed = 0)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Seat = seat ?? throw new ArgumentNullException(nameof(seat));

            if (discountAmount < 0)
                throw new ArgumentException("Discount cannot be negative.", nameof(discountAmount));
            if (bonusPointsUsed < 0)
                throw new ArgumentException("Bonus points cannot be negative.", nameof(bonusPointsUsed));

            DiscountAmount = discountAmount;
            BonusPointsUsed = bonusPointsUsed;

            All.Add(this);
        }

        private decimal CalculateFinalPrice()
        {
            decimal price = Seat.FinalSeatPrice;

            // Apply discount 
            price -= DiscountAmount;
            price -= BonusPointsUsed;
            
            return price < 0 ? 0 : price;
        }

        public void BookTicket()
        {
            if (IsBooked)
                throw new InvalidOperationException("Ticket is already booked.");

            IsBooked = true;

          //we will probably need to pass a cashier object here for box office orders
        }
        
        public static IReadOnlyList<Ticket> ListAll() => All.AsReadOnly();
    }
}
