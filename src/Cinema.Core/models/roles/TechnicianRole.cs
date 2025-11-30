using System.Text.Json.Serialization;
using Cinema.Core.models.operations;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.roles;

public sealed class TechnicianRole : EmployeeRole
{
    // --------------------------------------------------------
    // Fields
    // --------------------------------------------------------

    private string _degree = null!;

    [JsonIgnore]
    private readonly List<Session> _sessions = new();

    [JsonIgnore]
    private readonly List<Equipment> _equipment = new();

    // --------------------------------------------------------
    // Properties
    // --------------------------------------------------------

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

    // Constructor

    public TechnicianRole(string degree, bool isOnCall)
    {
        Degree = degree;
        IsOnCall = isOnCall;
    }


   
    public void AssignToSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Contains(session))
            return;

        _sessions.Add(session);
        session.AddTechnicianInternal(this); // reverse connection
    }

    
    public void RemoveFromSession(Session session)
    {
        if (session == null)
            throw new ArgumentNullException(nameof(session));

        if (_sessions.Count <= 1)
            throw new InvalidOperationException(
                "Technician must be assigned to at least one session (1..* multiplicity).");

        if (_sessions.Remove(session))
        {
            session.RemoveTechnicianInternal(this); //  reverse connection
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




    public void AssignToEquipment(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        if (_equipment.Contains(equipment))
            return;

        _equipment.Add(equipment);
        equipment.AddTechnicianInternal(this); // 🔁 reverse connection
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
            equipment.RemoveTechnicianInternal(this); //  reverse connection
        }
    }

    internal void AddEquipmentInternal(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        if (!_equipment.Contains(equipment))
            _equipment.Add(equipment);
    }

    internal void RemoveEquipmentInternal(Equipment equipment)
    {
        if (equipment == null)
            throw new ArgumentNullException(nameof(equipment));

        _equipment.Remove(equipment);
    }
}
