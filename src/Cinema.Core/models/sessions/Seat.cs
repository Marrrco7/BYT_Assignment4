using System.Text.Json;
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
        // =-=-=-=- Static extent =-=-=-=-

        private static readonly List<Seat> _all = new();
        public static IReadOnlyList<Seat> All => _all.AsReadOnly();

        // =-=-=-=- Fields =-=-=-=-

        private SeatType _type;
        private decimal _normalPrice;
        private bool _isAccessible;
        private decimal _ticketMultiplier;

        [JsonIgnore]
        private readonly List<Ticket> _tickets = new();

        // =-=-=-=- Properties =-=-=-=-

        public SeatType Type => _type;
        public decimal NormalPrice => _normalPrice;
        public bool IsAccessible => _isAccessible;
        public decimal TicketMultiplier => _ticketMultiplier;

        [JsonIgnore]
        public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

        // =-=-=-=- Constructors =-=-=-=-

        public Seat(
            SeatType type,
            decimal normalPrice,
            bool isAccessible,
            decimal? ticketMultiplier = null)
        {
            var multiplier = ticketMultiplier ?? 1.8m;
            Initialize(type, normalPrice, isAccessible, multiplier, addToExtent: true);
        }

        [JsonConstructor]
        public Seat(
            SeatType type,
            decimal normalPrice,
            bool isAccessible,
            decimal ticketMultiplier)
        {
            Initialize(type, normalPrice, isAccessible, ticketMultiplier, addToExtent: false);
        }

        private void Initialize(
            SeatType type,
            decimal normalPrice,
            bool isAccessible,
            decimal ticketMultiplier,
            bool addToExtent)
        {
            SetType(type);
            SetNormalPrice(normalPrice);
            SetAccessibility(isAccessible);
            SetTicketMultiplier(ticketMultiplier);

            if (addToExtent)
                _all.Add(this);
        }

        // =-=-=-=- Validation helpers  =-=-=-=-

        private void SetType(SeatType type)
        {
            _type = type;
        }

        private void SetNormalPrice(decimal price)
        {
            if (price < 0)
                throw new ArgumentException("Price cannot be negative.", nameof(NormalPrice));

            _normalPrice = price;
        }

        private void SetAccessibility(bool accessible)
        {
            _isAccessible = accessible;
        }

        private void SetTicketMultiplier(decimal multiplier)
        {
            if (multiplier <= 0)
                throw new ArgumentException("Ticket multiplier must be positive.", nameof(TicketMultiplier));

            _ticketMultiplier = multiplier;
        }

        // =-=-=-=- Association  Ticket =-=-=-=-

        internal void AddTicketInternal(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            if (!_tickets.Contains(ticket))
                _tickets.Add(ticket);
        }

        internal void RemoveTicketInternal(Ticket ticket)
        {
            if (ticket == null)
                throw new ArgumentNullException(nameof(ticket));

            _tickets.Remove(ticket);
        }

        // =-=-=-=- Business logic =-=-=-=-

        public decimal CalculateFinalSeatPrice()
        {
            return _type == SeatType.Vip
                ? _normalPrice * _ticketMultiplier
                : _normalPrice;
        }

        // =-=-=-=- Persistence =-=-=-=-

        public static void SaveToFile(string filePath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            var json = JsonSerializer.Serialize(All, options);
            File.WriteAllText(filePath, json);
        }

        public static void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;

            var json = File.ReadAllText(filePath);
            var seats = JsonSerializer.Deserialize<List<Seat>>(json);

            _all.Clear();
            if (seats != null)
                _all.AddRange(seats);
        }
    }
}
