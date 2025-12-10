using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.roles;
using Cinema.Core.models.sessions;

namespace Cinema.Core.models.operations;

public enum EquipmentType
{
    Audio,
    Projection,
    Lighting,
    Network,
    Storage
}

public class Equipment
{
    // Static extent
    private static readonly List<Equipment> _all = new();
    public static IReadOnlyList<Equipment> All => _all.AsReadOnly();

    // Fields
    private EquipmentType Type { get; set; }
    private DateTime DateOfLastCheckUp { get; set; }

    [JsonIgnore]
    private readonly List<TechnicianRole> _technicians = new();

    private Hall _hall;
    public Hall Hall
    {
        get => _hall;
        private set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Hall), "Equipment must belong to a hall");

            _hall = value;
        }
    }

    [JsonIgnore]
    public IReadOnlyList<TechnicianRole> Technicians => _technicians.AsReadOnly();

    // Constructors
    public Equipment(EquipmentType type, DateTime dateOfLastCheckUp, Hall hall)
    {
        Type = type;
        DateOfLastCheckUp = dateOfLastCheckUp;
        Hall = hall;

        SetHall(_hall);
        
        _all.Add(this);
    }
    
    // Hall
    public void SetHall(Hall newHall)
    {
        if (newHall == null) 
            throw new ArgumentNullException(nameof(newHall), "Equipment must belong to a Hall.");

        if (_hall == newHall) return;
        
        if (_hall != null)
        {
            _hall.RemoveEquipment(this);
        }
        
        _hall = newHall;
        
        if (!_hall.Equipment.Contains(this))
        {
            _hall.AddEquipment(this);
        }
    }

    // Technician
    public void AddTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (_technicians.Contains(technician))
            return;

        _technicians.Add(technician);
        technician.AddEquipmentInternal(this); //  reverse connection
    }

    public void RemoveTechnician(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (_technicians.Count <= 1)
            throw new InvalidOperationException(
                "Equipment must have at least one technician responsible (1..* multiplicity).");

        if (_technicians.Remove(technician))
        {
            technician.RemoveEquipmentInternal(this); // reverse connection
        }
    }

    internal void AddTechnicianInternal(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        if (!_technicians.Contains(technician))
            _technicians.Add(technician);
    }

    internal void RemoveTechnicianInternal(TechnicianRole technician)
    {
        if (technician == null)
            throw new ArgumentNullException(nameof(technician));

        _technicians.Remove(technician);
    }

    // Business logic
    public void UpdateLastCheckUpDate(DateTime newDate)
    {
        if (newDate > DateTime.Now)
            throw new ArgumentException("Date of last check-up cannot be in the future");

        DateOfLastCheckUp = newDate;
        Console.WriteLine($"({Type}) check-up updated to {DateOfLastCheckUp:d}");
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
        var equipments = JsonSerializer.Deserialize<List<Equipment>>(json);

        _all.Clear();
        if (equipments != null)
        {
            _all.AddRange(equipments);
        }
    }
}
