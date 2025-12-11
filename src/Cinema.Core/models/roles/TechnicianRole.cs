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
            if (!_isDummy)
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Degree cannot be null, empty, or whitespace.", nameof(value));
                if (value.Length < 2)
                    throw new ArgumentException("Degree name must be at least 2 characters long.", nameof(value));
            }
            _degree = value;
        }
    }

    public bool IsOnCall { get; }

    [JsonIgnore] public IReadOnlyList<Session> Sessions => _sessions.AsReadOnly();
    [JsonIgnore] public IReadOnlyList<Equipment> EquipmentAssigned => _equipment.AsReadOnly();
    [JsonIgnore] public bool IsDummy => _isDummy;

    // --- Constructors ---
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
        
        var dummyEquipment = Equipment.CreateDummyForTechnician(this);
        _equipment.Add(dummyEquipment);
    }

    // Factories
    public static TechnicianRole CreateDummyForSession(Session session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));

        var dummyTech = new TechnicianRole("DUMMY", false, isDummy: true);
        dummyTech.AttachSessionDummy(session);
        return dummyTech;
    }
    
    public static TechnicianRole CreateDummyForEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        var dummyTech = new TechnicianRole("DUMMY", false, isDummy: true);
        dummyTech.AttachEquipmentDummy(equipment);
        return dummyTech;
    }

    // Session
    public void AddSession(Session session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        
        if (_sessions.Contains(session)) return;

        _sessions.Add(session);
        
        session.AddTechnician(this);
    }

    public void RemoveSession(Session session)
    {
        if (session == null) throw new ArgumentNullException(nameof(session));
        
        if (_sessions.Count <= 1 && !_isDummy)
             throw new InvalidOperationException("Technician must have at least one session.");

        if (!_sessions.Contains(session)) return;

        _sessions.Remove(session);
        
        session.RemoveTechnician(this);
    }
    
    public void AttachSessionDummy(Session session)
    {
        if (!_sessions.Contains(session)) _sessions.Add(session);
    }

    // Equipment
    public void AddEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));
        
        if (_equipment.Contains(equipment)) return;

        _equipment.Add(equipment);
        
        equipment.AddTechnician(this); 
    }

    public void RemoveEquipment(Equipment equipment)
    {
        if (equipment == null) throw new ArgumentNullException(nameof(equipment));

        if (_equipment.Count <= 1 && !_isDummy)
            throw new InvalidOperationException("Technician must be assigned to at least one equipment.");

        if (_equipment.Contains(equipment))
        {
            _equipment.Remove(equipment);
            
            equipment.RemoveTechnician(this);
        }
    }

    public void AttachEquipmentDummy(Equipment equipment)
    {
        if (!_equipment.Contains(equipment)) _equipment.Add(equipment);
    }
}