using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.roles;

namespace Cinema.Core.models.customers;

public enum ContractType
{
    FullTimeContract,
    PartTimeContract
}

public sealed class Employee : Person
{
    // Static extent
    private static readonly List<Employee> _all = new();
    public static IReadOnlyList<Employee> All => _all.AsReadOnly();

    // Fields
    private DateOnly _hiringDate;
    private string _phoneNumber;
    
    // Flattening: Discriminator
    public ContractType ContractType { get; private set; }

    // Basic properties
    public DateOnly HiringDate
    {
        get => _hiringDate;
        private set
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            if (value > today)
                throw new ArgumentException("Hiring date cannot be in the future.", nameof(value));

            _hiringDate = value;
        }
    }

    public string PhoneNumber
    {
        get => _phoneNumber;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Phone number cannot be empty.", nameof(value));

            _phoneNumber = value;
        }
    }
    
    // FLATTENED CONTRACT FIELDS
    
    // Full-Time Contract
    public static readonly decimal MIN_SALARY = 500m;
    public static readonly decimal MAX_SALARY = 3500m;

    private decimal? _salary;
    public decimal? Salary
    {
        get => _salary;
        set
        {
            if (ContractType == ContractType.PartTimeContract)
            {
                if (value != null)
                    throw new InvalidOperationException("Part-time employees cannot be assigned a Salary.");
                
                _salary = null;
                return;
            }
            
            if (value == null)
                throw new ArgumentNullException(nameof(Salary), "Full-time employees must have a Salary.");

            if (value < MIN_SALARY)
                throw new ArgumentOutOfRangeException(nameof(Salary), $"Salary must be at least {MIN_SALARY}.");
        
            if (value > MAX_SALARY)
                throw new ArgumentOutOfRangeException(nameof(Salary), $"Salary cannot exceed {MAX_SALARY}.");
        
            if (decimal.Round(value.Value, 2) != value)
                throw new ArgumentException("Salary must have at most two decimal places.", nameof(Salary));
            
            _salary = value;
        }
    }
    
    private bool? _hasBenefitsPlan;
    public bool? HasBenefitsPlan 
    { 
        get => _hasBenefitsPlan; 
        private set
        {
            if (ContractType == ContractType.PartTimeContract)
            {
                if (value != null && value == true)
                    throw new InvalidOperationException("Part-time employees cannot have a Benefits Plan.");
                _hasBenefitsPlan = null;
                return;
            }
            
            _hasBenefitsPlan = value ?? false;
        }
    }
    
    
    // Part-Time Contract
    public static readonly decimal MIN_HOURLY_RATE = 5.00m;
    public static readonly decimal MAX_HOURLY_RATE = 50.00m;
    public static readonly int MIN_WEEKLY_HOURS = 1;
    public static readonly int MAX_WEEKLY_HOURS = 30;
    
    private decimal? _hourlyRate;
    public decimal? HourlyRate { get => _hourlyRate;
        set
        {
            if (ContractType == ContractType.FullTimeContract)
            {
                if (value != null)
                    throw new InvalidOperationException("Full-time employees cannot be assigned an Hourly Rate.");
                
                _hourlyRate = null;
                return;
            }
            
            
            if (value == null)
                throw new ArgumentNullException(nameof(HourlyRate), "Part-time employees must have an Hourly Rate.");

            if (value < MIN_HOURLY_RATE || value > MAX_HOURLY_RATE)
                throw new ArgumentOutOfRangeException(nameof(HourlyRate),
                    $"Hourly rate must be between {MIN_HOURLY_RATE} and {MAX_HOURLY_RATE}.");
            
            _hourlyRate = value;
        }
    }

    private int? _maxWeekHours;
    public int? MaxWeekHours
    {
        get => _maxWeekHours;
        set
        {
            if (ContractType == ContractType.FullTimeContract)
            {
                if (value != null)
                    throw new InvalidOperationException("Full-time employees cannot be assigned Max Weekly Hours.");
                
                _maxWeekHours = null;
                return;
            }
            
            if (value == null)
                throw new ArgumentNullException(nameof(MaxWeekHours), "Part-time employees must have Max Weekly Hours.");

            if (value < MIN_WEEKLY_HOURS || value > MAX_WEEKLY_HOURS)
                throw new ArgumentOutOfRangeException(nameof(MaxWeekHours),
                    $"Weekly hours must be between {MIN_WEEKLY_HOURS} and {MAX_WEEKLY_HOURS}.");

            _maxWeekHours = value;
        }
    }

    // ROLES (Composition)
    private TechnicianRole? _technicianRole;
    private CleanerRole? _cleanerRole;
    private CashierRole? _cashierRole;

    public TechnicianRole? TechnicianRole => _technicianRole;
    public CleanerRole? CleanerRole => _cleanerRole;
    public CashierRole? CashierRole => _cashierRole;
    
    
    // REFLEX ASSOCIATION
    [JsonIgnore]
    public Employee? Supervisor { get; private set; }

    [JsonIgnore]
    private readonly List<Employee> _subordinates = new();

    [JsonIgnore]
    public IReadOnlyList<Employee> Subordinates => _subordinates.AsReadOnly();

    // Constructor
    [JsonConstructor]
    public Employee(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        DateOnly hiringDate,
        string phoneNumber,
        ContractType contractType,
        decimal? salary = null,
        bool? hasBenefitsPlan = null,
        decimal? hourlyRate = null,
        int? maxWeekHours = null)
        : base(firstName, lastName, dateOfBirth)
    {
        HiringDate  = hiringDate;
        PhoneNumber = phoneNumber;
        
        ContractType = contractType;
        
        Salary = salary;
        HourlyRate = hourlyRate;
        MaxWeekHours = maxWeekHours;
        HasBenefitsPlan = hasBenefitsPlan;

        _all.Add(this);
    }

    // ROLES
    // Technician Composition
    public void AddTechnicianRole(TechnicianRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_technicianRole != null)
            throw new InvalidOperationException("Employee already has Technician role.");

        if (role.Employee != this)
            throw new InvalidOperationException("Composition error: role belongs to another employee.");

        _technicianRole = role;
    }
    
    public void RemoveTechnicianRole(TechnicianRole role)
    {
        if (_technicianRole != role)
            throw new InvalidOperationException("Composition error.");

        _technicianRole = null;
    }
    
    // Cleaner Composition
    public void AddCleanerRole(CleanerRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_cleanerRole != null)
            throw new InvalidOperationException("Employee already has Cleaner role.");

        if (role.Employee != this)
            throw new InvalidOperationException("Composition error: role belongs to another employee.");

        _cleanerRole = role;
    }

    public void RemoveCleanerRole(CleanerRole role)
    {
        if (_cleanerRole != role)
            throw new InvalidOperationException("Composition error.");

        _cleanerRole = null;
    }
    
    // Cashier Composition
    public void AddCashierRole(CashierRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (_cashierRole != null)
            throw new InvalidOperationException("Employee already has Cashier role.");

        if (role.Employee != this)
            throw new InvalidOperationException("Composition error: role belongs to another employee.");

        _cashierRole = role;
    }

    public void RemoveCashierRole(CashierRole role)
    {
        if (_cashierRole != role)
            throw new InvalidOperationException("Composition error.");

        _cashierRole = null;
    }
    
    // Delete Employee
    public static bool DeleteEmployee(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));
        
        if (employee.Supervisor != null)
        {
            employee.RemoveSupervisor(); 
        }
        
        foreach (var subordinate in employee.Subordinates.ToList())
        {
            subordinate.RemoveSupervisor();
        }
        
        employee._technicianRole?.DeletePart();
        employee._cleanerRole?.DeletePart();
        employee._cashierRole?.DeletePart();

        employee._technicianRole = null;
        employee._cleanerRole = null;
        employee._cashierRole = null;
        
        return _all.Remove(employee);
    }


    // Reflex association 
    public void SetSupervisor(Employee? supervisor)
    {
        if (ReferenceEquals(supervisor, this))
            throw new InvalidOperationException("Employee cannot be their own supervisor.");

        if (Supervisor == supervisor)
            return;

        if (Supervisor != null)
        {
            var oldSupervisor = Supervisor;

            if (oldSupervisor._subordinates.Contains(this))
            {
                oldSupervisor.RemoveSubordinate(this);
            }
        }

        if (supervisor != null)
        {
            Supervisor = supervisor;

            if (!supervisor._subordinates.Contains(this))
            {
                supervisor.AddSubordinate(this);
            }
        }
        else
        {
            Supervisor = null;
        }
    }

    public void RemoveSupervisor()
    {
        SetSupervisor(null);
    }
    
    // Reflex Association
    public void AddSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (ReferenceEquals(employee, this))
            throw new InvalidOperationException("Employee cannot supervise themselves.");

        if (_subordinates.Contains(employee))
        {
            if (employee.Supervisor != this)
            {
                employee.SetSupervisor(this);
            }
            return;
        }

        _subordinates.Add(employee);

        if (employee.Supervisor != this)
        {
            employee.SetSupervisor(this);
        }
    }

    public void RemoveSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (!_subordinates.Contains(employee))
            return;

        _subordinates.Remove(employee);

        if (employee.Supervisor == this)
        {
            employee.SetSupervisor(null);
        }
    }

    // Persistence
    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Serialize(_all, options);
        File.WriteAllText(filePath, json);
    }

    public static void LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return;

        var json = File.ReadAllText(filePath);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var employees = JsonSerializer.Deserialize<List<Employee>>(json, options);

        if (employees != null)
        {
            _all.Clear();
            _all.AddRange(employees);
        }
    }
}
