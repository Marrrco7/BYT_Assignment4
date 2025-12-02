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
    private readonly List<EmployeeRole> _roles = new();
    public IReadOnlyList<EmployeeRole> Roles => _roles.AsReadOnly();

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
        HiringDate = hiringDate;
        PhoneNumber = phoneNumber;
        Contract = contract;

        _all.Add(this);
    }

    // Roles
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

    // REFLEX ASSOCIATION — Supervisor side

    public void SetSupervisor(Employee? supervisor)
    {
        if (ReferenceEquals(supervisor, this))
            throw new InvalidOperationException("Employee cannot be their own supervisor.");

        if (Supervisor == supervisor)
            return;

        if (Supervisor != null)
        {
            var oldSupervisor = Supervisor;
            Supervisor = null;

            oldSupervisor._subordinates.Remove(this);
        }

        if (supervisor != null)
        {
            Supervisor = supervisor;
            if (!supervisor._subordinates.Contains(this))
                supervisor._subordinates.Add(this);
        }
    }

    
    public void RemoveSupervisor()
    {
        SetSupervisor(null);
    }

    internal void SetSupervisorInternal(Employee? supervisor)
    {
        if (ReferenceEquals(supervisor, this))
            throw new InvalidOperationException("Employee cannot be their own supervisor.");

        Supervisor = supervisor;
    }

    // REFLEX ASSOCIATION — Subordinates side

    public void AddSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (ReferenceEquals(employee, this))
            throw new InvalidOperationException("Employee cannot supervise themselves.");

        if (_subordinates.Contains(employee))
            return;

        _subordinates.Add(employee);
        employee.SetSupervisorInternal(this); // internal reverse connection
    }

    public void RemoveSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (_subordinates.Remove(employee))
        {
            if (employee.Supervisor == this)
                employee.SetSupervisorInternal(null);
        }
    }

    // Contract
    public void ChangeContract(EmploymentContract newContract)
    {
        Contract = newContract;
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
