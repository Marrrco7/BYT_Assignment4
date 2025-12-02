using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.sales;

public class Promotion
{
    private static readonly List<Promotion> _all = new();
    public static IReadOnlyList<Promotion> All => _all.AsReadOnly();

    [JsonIgnore]
    private readonly List<Session> _sessions = new();

    [JsonIgnore]
    private readonly List<Ticket> _tickets = new();

    [JsonIgnore]
    public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<Ticket> Tickets => _tickets.AsReadOnly();

    private DateTime _validFrom;
    private DateTime _validTo;

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

    public string Description { get; private set; } = string.Empty;
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

    public decimal DiscountValue => DiscountAmount;

    // =-=-=-=- Session =-=-=-=-

    public void AddSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Contains(session))
            return;

        _sessions.Add(session);
        session.AddPromotionInternal(this); // reverse
    }

    public void RemoveSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Remove(session))
        {
            session.RemovePromotionInternal(this); // reverse
        }
    }

    internal void AddSessionInternal(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!_sessions.Contains(session))
            _sessions.Add(session);
    }

    internal void RemoveSessionInternal(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        _sessions.Remove(session);
    }

    public bool AppliesTo(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        return _sessions.Contains(session);
    }

    // =-=-=-=- Ticket =-=-=-=-

    public void AddTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (_tickets.Contains(ticket))
            return;

        _tickets.Add(ticket);
        ticket.SetPromotionInternal(this); // reverse
    }

    public void RemoveTicket(Ticket ticket)
    {
        if (ticket == null)
            throw new ArgumentNullException(nameof(ticket));

        if (_tickets.Remove(ticket))
        {
            if (ReferenceEquals(ticket.Promotion, this))
                ticket.SetPromotionInternal(null);
        }
    }

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
        var promotions = JsonSerializer.Deserialize<List<Promotion>>(json);

        _all.Clear();
        if (promotions != null) _all.AddRange(promotions);
    }
}
