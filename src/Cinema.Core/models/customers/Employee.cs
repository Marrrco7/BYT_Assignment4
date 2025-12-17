using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.contract;
using Cinema.Core.models.roles;

namespace Cinema.Core.models.customers;

public sealed class Employee : Person
{
    // Static extent
    private static readonly List<Employee> _all = new();
    public static IReadOnlyList<Employee> All => _all.AsReadOnly();

    // Fields
    private DateOnly _hiringDate;
    private string _phoneNumber;
    private EmploymentContract _contract;

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

    public EmploymentContract Contract
    {
        get => _contract;
        private set => _contract = value ?? throw new ArgumentNullException(nameof(value));
    }

    // Employee roles
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
    public Employee(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        DateOnly hiringDate,
        string phoneNumber,
        EmploymentContract contract)
        : base(firstName, lastName, dateOfBirth)
    {
        HiringDate  = hiringDate;
        PhoneNumber = phoneNumber;
        Contract    = contract;

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

    // Contract
    public void ChangeContract(EmploymentContract newContract)
    {
        Contract = newContract ?? throw new ArgumentNullException(nameof(newContract));
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
