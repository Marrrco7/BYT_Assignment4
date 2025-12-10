using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions
{
    public class Ticket
    {
        public static List<Ticket> All { get; } = new();

        // Associations
        public Session? Session { get; private set; }
        public Seat Seat { get; }
        public Order Order { get; internal set; }
        public Promotion? Promotion { get; private set; }

        public bool IsBooked { get; private set; }

        public Ticket(Session session, Seat seat, Order order)
        {
            Seat  = seat  ?? throw new ArgumentNullException(nameof(seat));
            Order = order ?? throw new ArgumentNullException(nameof(order));

            All.Add(this);

            // reverse connections
            Seat.AddTicketInternal(this);

            SetSession(session ?? throw new ArgumentNullException(nameof(session)));
        }

        // ------------ Associations

        public void SetSession(Session? session)
        {
            if (Session == session)
                return;

            if (Session != null)
            {
                var oldSession = Session;
                Session = null; 

                if (oldSession.Tickets.Contains(this))
                {
                    oldSession.RemoveTicket(this);
                }
            }

            if (session != null)
            {
                Session = session;

                if (!session.Tickets.Contains(this))
                {
                    session.AddTicket(this);
                }
            }
            else
            {
                Session = null;
            }
        }

        // ------------ Associations: Promotion 

        public void SetPromotion(Promotion? promotion)
        {
            if (Promotion == promotion)
                return;

            if (Promotion != null)
            {
                var oldPromotion = Promotion;
                Promotion = null;

                if (oldPromotion.Tickets.Contains(this))
                {
                    oldPromotion.RemoveTicket(this);
                }
            }

            if (promotion != null)
            {
                Promotion = promotion;

                if (!promotion.Tickets.Contains(this))
                {
                    promotion.AddTicket(this);
                }
            }
            else
            {
                Promotion = null;
            }
        }

        // ------------ Business logic ------------

        public decimal CalculateFinalPrice(decimal bonusPointsUsed = 0)
        {
            decimal price = Seat.CalculateFinalSeatPrice();

            Promotion? promo = Promotion;

            if (promo == null && Session != null)
            {
                promo = Session.Promotions.FirstOrDefault(p => p.IsActive());
            }

            if (promo != null && promo.IsActive())
            {
                price -= promo.DiscountValue;
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

        // ------------ Composition helpers ------------

        internal static void DeleteOrderPart(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (!All.Contains(ticket))
                return;

            ticket.Order.RemoveTicketInternal(ticket);

            ticket.SetSession(null);
            ticket.SetPromotion(null);

            All.Remove(ticket);
        }

        internal static void DeleteSeatPart(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (!All.Contains(ticket))
                return;

            ticket.Seat.RemoveTicketInternal(ticket);

            ticket.SetSession(null);
            ticket.SetPromotion(null);

            All.Remove(ticket);
        }
    }
}
