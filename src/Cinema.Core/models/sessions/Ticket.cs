using System.Text.Json;
using Cinema.Core.models.operations;
using Cinema.Core.models.sales;

namespace Cinema.Core.models.sessions;

public class Ticket
{
    // Extent
    private static readonly List<Ticket> _all = new();
    public static IReadOnlyList<Ticket> All => _all.AsReadOnly();

    // Associations
    private Session _session;
    public Session Session { get => _session; private set => _session = value; }
    private Seat _seat;
    public Seat Seat { get => _seat; private set => _seat = value; }
    public Order Order { get; private set; }
    private Promotion? _promotion;
    public Promotion? Promotion
    {
        get => _promotion;
        private set => _promotion = value;
    }

    public bool IsBooked { get; private set; }

    public Ticket(Session session, Seat seat, Order order)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (seat == null) throw new ArgumentNullException(nameof(seat));
        Order = order ?? throw new ArgumentNullException(nameof(order));

       _session = session;
        _seat = seat;

        _all.Add(this);

        SetSession(session);
        SetSeat(seat);

        Order.AddTicket(this);
    }

    // Session
    public void SetSession(Session newSession)
    {
        if (newSession == null)
            throw new ArgumentNullException(nameof(newSession), "Ticket must be assigned to a Session.");

        if (_session == newSession) return;

        if (_session != null && _session.Tickets.Contains(this))
        {
            _session.RemoveTicket(this);
        }

        _session = newSession;

        if (!_session.Tickets.Contains(this))
        {
            _session.AddTicket(this);
        }
    }

    // ------------ Associations: Promotion 

    public void SetPromotion(Promotion? newPromotion)
    {
        if (_promotion == newPromotion) return;

        if (_promotion != null && _promotion.Tickets.Contains(this))
        {
            _promotion.RemoveTicket(this);
        }

        _promotion = newPromotion;

        if (_promotion != null && !_promotion.Tickets.Contains(this))
        {
            _promotion.AddTicket(this);
        }
    }

    // ------------ Associations: Seat
    public void SetSeat(Seat newSeat)
    {
        if (newSeat == null)
            throw new ArgumentNullException(nameof(newSeat), "Ticket must be assigned to a Seat.");

        if (_seat == newSeat) return;

        if (_seat != null && _seat.Tickets.Contains(this))
        {
            _seat.RemoveTicket(this);
        }

        // connect new
        _seat = newSeat;

        // reverse connection
        if (!_seat.Tickets.Contains(this))
        {
            _seat.AddTicket(this);
        }
    }


    // ------------ Business logic ------------
    public decimal CalculateFinalPrice(decimal bonusPointsUsed = 0)
    {
        var price = Seat.CalculateFinalSeatPrice();

        var promo = Promotion;

        if (promo == null && Session != null) promo = Session.Promotions.FirstOrDefault(p => p.IsActive());

        if (promo != null && promo.IsActive()) price -= promo.DiscountValue;

        if (bonusPointsUsed > 0) price -= bonusPointsUsed;

        return price < 0 ? 0 : price;
    }

    public void BookTicket()
    {
        if (IsBooked)
            throw new InvalidOperationException("Ticket is already booked.");

        IsBooked = true;
    }

    // Composition
    public void DeletePart()
    {
        _all.Remove(this);

        if (Order != null)
        {
            Order.RemoveTicket(this);
        }

       if (_session != null)
        {
            if (_session.Tickets.Contains(this))
            {
                _session.RemoveTicket(this);
            }
            _session = null!;
        }

        SetPromotion(null);

        if (_seat != null)
        {
            if (_seat.Tickets.Contains(this))
            {
                _seat.RemoveTicket(this);
            }
            _seat = null!;
        }
    }

    // Persistence
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
        var tickets = JsonSerializer.Deserialize<List<Ticket>>(json);

        _all.Clear();
        if (tickets != null)
        {
            _all.AddRange(tickets);
        }
    }
}