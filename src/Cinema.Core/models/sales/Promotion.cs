using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.sales;

public class Promotion
{
    private static readonly List<Promotion> _all = new();
    public static IReadOnlyList<Promotion> All => _all.AsReadOnly();


    [JsonIgnore]
    private readonly List<Ticket> _tickets = new();

    [JsonIgnore]
    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

    [JsonIgnore]
    private readonly List<Session> _sessions = new();

    [JsonIgnore]
    public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();


    private DateTime _validFrom;

    public DateTime ValidFrom
    {
        get => _validFrom;
        private set
        {
            if (_validTo != DateTime.MinValue && value > _validTo)
                throw new ArgumentException("ValidFrom date cannot be after ValidTo date.");
            _validFrom = value;
        }
    }

    private DateTime _validTo;

    public DateTime ValidTo
    {
        get => _validTo;
        private set
        {
            if (value < ValidFrom)
                throw new ArgumentException("ValidTo date cannot be before ValidFrom date.");
            _validTo = value;
        }
    }

    public string Description { get; private set; }
    public decimal DiscountAmount { get; private set; }

    public Promotion() { }

    public Promotion(DateTime validFrom, DateTime validTo, string description, decimal discountAmount)
    {
        ValidFrom = validFrom;
        ValidTo = validTo;
        Description = description;
        DiscountAmount = discountAmount;
        
        _all.Add(this);
    }

    public bool IsActive()
    {
        var today = DateTime.Today;
        return today >= ValidFrom.Date && today <= ValidTo.Date;
    }

    // ------------  Associations: Tickets 

    public void AddTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (_tickets.Contains(ticket))
        {
            if (ticket.Promotion != this)
            {
                ticket.SetPromotion(this);
            }
            return;
        }

        _tickets.Add(ticket);

        if (ticket.Promotion != this)
        {
            ticket.SetPromotion(this);
        }
    }

    public void RemoveTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (!_tickets.Contains(ticket))
            return;

        _tickets.Remove(ticket);

        if (ticket.Promotion == this)
        {
            ticket.SetPromotion(null);
        }
    }

    // ------------  Associations: Sessions 

    public void AddSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Contains(session))
        {
            if (!session.Promotions.Contains(this))
            {
                session.AddPromotion(this);
            }
            return;
        }

        _sessions.Add(session);

        if (!session.Promotions.Contains(this))
        {
            session.AddPromotion(this);
        }
    }

    public void RemoveSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!_sessions.Contains(session))
            return;

        _sessions.Remove(session);

        if (session.Promotions.Contains(this))
        {
            session.RemovePromotion(this);
        }
    }

    // ------------ Persistence -

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
        var promotions = JsonSerializer.Deserialize<List<Promotion>>(json);

        _all.Clear();
        if (promotions != null) _all.AddRange(promotions);
    }
    
    public decimal DiscountValue
    {
        get { return DiscountAmount; }
    }
}
