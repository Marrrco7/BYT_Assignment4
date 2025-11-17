using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.contract;
using Cinema.Core.models.roles;

namespace Cinema.Core.models.customers;

public sealed class Employee : Person
{
    private static readonly List<Employee> _all = new();
    public static IReadOnlyList<Employee> All => _all.AsReadOnly();

    private DateOnly _hiringDate;
    private string _phoneNumber;
    private EmploymentContract _contract;

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

    private readonly List<EmployeeRole> _roles = new();
    public IReadOnlyList<EmployeeRole> Roles => _roles.AsReadOnly();

    [JsonIgnore]
    public Employee? Supervisor { get; private set; }

    [JsonIgnore]
    private readonly List<Employee> _subordinates = new();
    [JsonIgnore]
    public IReadOnlyList<Employee> Subordinates => _subordinates.AsReadOnly();

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

    public void ChangeContract(EmploymentContract newContract)
    {
        Contract = newContract; 
    }

    public void AddRole(EmployeeRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!_roles.Contains(role))
            _roles.Add(role);
    }

    public void RemoveRole(EmployeeRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        _roles.Remove(role);
    }

    public void AddSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (ReferenceEquals(employee, this))
            throw new InvalidOperationException("Employee cannot supervise themselves.");

        if (!_subordinates.Contains(employee))
        {
            _subordinates.Add(employee);
            employee.Supervisor = this;
        }
    }

    public void RemoveSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (_subordinates.Remove(employee) && employee.Supervisor == this)
        {
            employee.Supervisor = null;
        }
    }

    public void AssignSupervisor(Employee supervisor)
    {
        if (supervisor == null)
            throw new ArgumentNullException(nameof(supervisor));

        if (ReferenceEquals(supervisor, this))
            throw new InvalidOperationException("Employee cannot be their own supervisor.");

        Supervisor = supervisor;
        if (!supervisor._subordinates.Contains(this))
            supervisor._subordinates.Add(this);
    }

    public void RemoveSupervisor()
    {
        if (Supervisor == null)
            throw new InvalidOperationException("This employee does not have a supervisor.");

        Supervisor._subordinates.Remove(this);
        Supervisor = null;
    }

    
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
