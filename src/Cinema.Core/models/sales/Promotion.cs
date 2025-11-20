using System.Text.Json;

namespace Cinema.Core.models.sales;

public class Promotion
{
    private static readonly List<Promotion> _all = new();
    public static IReadOnlyList<Promotion> All => _all.AsReadOnly();

    private DateTime _validFrom;

    public DateTime ValidFrom
    {
        get => _validFrom;
        private set
        {
            if (value > ValidTo)
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

    private string Description { get; set; }
    private decimal DiscountAmount { get; set; }

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