using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions
{
    public class Ticket
    {
        public static List<Ticket> All { get; } = new();
        public Session Session { get; }
        public Seat Seat { get; }
        public bool IsBooked { get; private set; }

        public Ticket(Session session, Seat seat)
        {
            Session = session ?? throw new ArgumentNullException(nameof(session));
            Seat = seat ?? throw new ArgumentNullException(nameof(seat));

            All.Add(this);
        }

        public decimal CalculateFinalPrice(decimal bonusPointsUsed = 0)
        {
            decimal price = Seat.CalculateFinalSeatPrice();

            Promotion? activePromo = Promotion.All
                .FirstOrDefault(p => p.IsActive());

            if (activePromo != null)
            {
                price -= activePromo.GetDiscountAmount(); 
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
    }
}