using System.Text.Json.Serialization;
using Cinema.Core.models.operations;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.roles;

public sealed class TechnicianRole : EmployeeRole
{
    // Fields

    private string _degree = null!;

    [JsonIgnore]
    private readonly List<Session> _sessions = new();

    [JsonIgnore]
    private readonly List<Equipment> _equipment = new();

    private readonly bool _isDummy;

    // Properties

    public string Degree
    {
        get => _degree;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Degree cannot be null, empty, or whitespace.", nameof(value));

            if (value.Length < 2)
                throw new ArgumentException("Degree name must be at least 2 characters long.", nameof(value));

            if (!value.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c == '-'))
                throw new ArgumentException("Degree can only contain letters, spaces, or hyphens.", nameof(value));

            _degree = value;
        }
    }

    public bool IsOnCall { get; }

    [JsonIgnore]
    public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();

    [JsonIgnore]
    public IReadOnlyList<Equipment> EquipmentAssigned => _equipment.AsReadOnly();

    [JsonIgnore]
    public bool IsDummy => _isDummy;


    private TechnicianRole(string degree, bool isOnCall, bool isDummy)
    {
        _isDummy = isDummy;
        Degree   = degree;
        IsOnCall = isOnCall;
    }

    public TechnicianRole(string degree, bool isOnCall)
        : this(degree, isOnCall, false)
    {
        var dummySession = Session.CreateDummyForTechnician(this);
        _sessions.Add(dummySession);
    }

    public static TechnicianRole CreateDummyForSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        var dummyTech = new TechnicianRole("DUMMY_TECHNICIAN", false, isDummy: true);

        dummyTech.AttachSessionDummy(session);
        session.AttachTechnicianDummy(dummyTech);

        return dummyTech;
    }

    // -------- Ассоциация TechnicianRole 

    public void AssignToSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (session.IsDummy)
            throw new InvalidOperationException("Cannot assign technician to a dummy session directly.");

        if (_sessions.Contains(session))
        {
            if (!session.Technicians.Contains(this))
            {
                session.AddTechnician(this);
            }
            return;
        }

        _sessions.Add(session);

        if (!session.Technicians.Contains(this))
        {
            session.AddTechnician(this);
        }
    }

    public void RemoveFromSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (session.IsDummy)
            return;

        if (!_sessions.Contains(session))
            return;

        _sessions.Remove(session);

        if (session.Technicians.Contains(this))
        {
            session.RemoveTechnician(this);
        }
    }

    public void AttachSessionDummy(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (!_sessions.Contains(session))
        {
            _sessions.Add(session);
        }
    }

    // -------- Ассоциация  Equipment 

    public void AssignToEquipment(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        if (_equipment.Contains(equipment))
            return;

        _equipment.Add(equipment);
        equipment.AddTechnicianInternal(this); // reverse connection
    }

    public void RemoveFromEquipment(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        if (_equipment.Count <= 1)
            throw new InvalidOperationException(
                "Technician must be assigned to at least one equipment (1..* multiplicity).");

        if (_equipment.Remove(equipment))
        {
            equipment.RemoveTechnicianInternal(this); // reverse connection
        }
    }

    public void AddEquipmentInternal(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        if (!_equipment.Contains(equipment))
            _equipment.Add(equipment);
    }

    public void RemoveEquipmentInternal(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        _equipment.Remove(equipment);
    }
}
