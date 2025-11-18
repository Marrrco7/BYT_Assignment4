using System.Text.Json;
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
    private static readonly List<Equipment> _all = new();
    public static IReadOnlyList<Equipment> All => _all.AsReadOnly();

    private EquipmentType Type { get; set; }
    private DateTime _dateOfLastCheckUp;
    public DateTime DateOfLastCheckUp
    {
        get => _dateOfLastCheckUp;
        private set
        {
            if (value > DateTime.Now)
                throw new ArgumentException("Date of last check-up cannot be in the future");
            _dateOfLastCheckUp = value;
        } 
    }
    
    private Hall _hall = null!;
    private Hall Hall
    {
        get => _hall;
        set
        {
            if (value == null)
                throw new ArgumentNullException(nameof(Hall), "Equipment must belong to a hall");

            _hall = value;
        }
    }
    
    public Equipment(EquipmentType type, DateTime dateOfLastCheckUp, Hall hall)
    {
        Type = type;
        DateOfLastCheckUp = dateOfLastCheckUp;
        Hall = hall;
        
        hall.AddEquipment(this);
        _all.Add(this);
    }
    
    public void UpdateLastCheckUpDate(DateTime newDate)
    {
        if (newDate > DateTime.Now)
            throw new ArgumentException("Date of last check-up cannot be in the future");

        DateOfLastCheckUp = newDate;
        Console.WriteLine($"({Type}) check-up updated to {DateOfLastCheckUp:d}");
    }
    
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