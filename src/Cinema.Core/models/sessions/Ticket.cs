using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions
{
    public class Ticket
    {
        // =-=-=-=- Static extent =-=-=-=-

        private static readonly List<Ticket> _all = new();
        public static IReadOnlyList<Ticket> All => _all.AsReadOnly();

        // =-=-=-=- Fields =-=-=-=-

        private Session _session = null!;
        private Seat _seat = null!;
        private Promotion? _promotion;
        private bool _isBooked;

        // =-=-=-=- Properties =-=-=-=-

        public Session Session => _session;
        public Seat Seat => _seat;
        public Promotion? Promotion => _promotion;
        public bool IsBooked => _isBooked;

        // =-=-=-=- Constructors =-=-=-=-

        public Ticket(Session session, Seat seat, Promotion? promotion = null)
        {
            Initialize(session, seat, promotion, isBooked: false, addToExtent: true);
        }

        [JsonConstructor]
        public Ticket(Session session, Seat seat, Promotion? promotion, bool isBooked)
        {
            Initialize(session, seat, promotion, isBooked, addToExtent: false);
        }

        private void Initialize(Session session,
                                Seat seat,
                                Promotion? promotion,
                                bool isBooked,
                                bool addToExtent)
        {
            if (session == null)
                throw new ArgumentNullException(nameof(session));
            if (seat == null)
                throw new ArgumentNullException(nameof(seat));

            if (addToExtent && _all.Any(t => ReferenceEquals(t.Session, session) &&
                                             ReferenceEquals(t.Seat, seat)))
            {
                throw new InvalidOperationException(
                    "Ticket for this seat in this session already exists.");
            }

            _session = session;
            _seat = seat;
            _promotion = promotion;
            _isBooked = isBooked;

            _session.AddTicketInternal(this);
            _promotion?.AddTicketInternal(this);

            if (addToExtent)
                _all.Add(this);
        }

        // =-=-=-=- Ticket - Promotion =-=-=-=-

        public void ApplyPromotion(Promotion promotion)
        {
            if (promotion == null)
                throw new ArgumentNullException(nameof(promotion));

            if (!promotion.AppliesTo(_session))
                throw new InvalidOperationException("This promotion does not apply to the ticket's session.");

            if (_promotion == promotion)
                return;

            _promotion?.RemoveTicketInternal(this);

            _promotion = promotion;
            promotion.AddTicketInternal(this);
        }

        public void RemovePromotion()
        {
            if (_promotion == null)
                return;

            var old = _promotion;
            _promotion = null;
            old.RemoveTicketInternal(this);
        }

        internal void SetPromotionInternal(Promotion? promotion)
        {
            _promotion = promotion;
        }

        // =-=-=-=- Бизнес-логика =-=-=-=-

        public decimal CalculateFinalPrice(decimal bonusPointsUsed = 0)
        {
            decimal price = _seat.CalculateFinalSeatPrice();

            Promotion? promo = _promotion;

            if (promo == null)
            {
                promo = Promotion.All
                    .FirstOrDefault(p => p.IsActive() && p.AppliesTo(_session));
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
            if (_isBooked)
                throw new InvalidOperationException("Ticket is already booked.");

            _isBooked = true;
        }

        // =-=-=-=- Persistence =-=-=-=-

        public static void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.Preserve
            };

            var json = JsonSerializer.Serialize(All, options);
            File.WriteAllText(filePath, json);
        }

        public static void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var tickets = JsonSerializer.Deserialize<List<Ticket>>(json, new JsonSerializerOptions
            {
                ReferenceHandler = ReferenceHandler.Preserve
            });

            _all.Clear();
            if (tickets != null)
                _all.AddRange(tickets);
        }
    }
}
