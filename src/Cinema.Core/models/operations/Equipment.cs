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
    private EquipmentType Type { get; set; }
    private DateTime DateOfLastCheckUp { get; set; }
    
    private Hall Hall { get; set; }
    
    public Equipment(EquipmentType type, DateTime dateOfLastCheckUp, Hall hall)
    {
        if (dateOfLastCheckUp > DateTime.Now)
            throw new ArgumentException("Date of last check-up cannot be in the future");
        
        Type = type;
        DateOfLastCheckUp = dateOfLastCheckUp;
        Hall = hall ?? throw new ArgumentNullException(nameof(hall), "Equipment must belong to a hall");

        // we should implement this method in Hall class
        hall.AddEquipment(this);
    }
    
    public void UpdateLastCheckUpDate(DateTime newDate)
    {
        if (newDate > DateTime.Now)
            throw new ArgumentException("Date of last check-up cannot be in the future");

        DateOfLastCheckUp = newDate;
        Console.WriteLine($"({Type}) check-up updated to {DateOfLastCheckUp:d}");
    }
}