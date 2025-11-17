using System.Text.Json;
using System.Text.Json.Serialization;
using Cinema.Core.models.contract;
using Cinema.Core.models.roles;

namespace Cinema.Core.models.customers;

public sealed class Employee : Person
{
    public static List<Employee> All { get; } = new();
    public DateOnly HiringDate { get; private set; }
    public string PhoneNumber { get; private set; }
    public EmploymentContract Contract { get; private set; }
    public List<EmployeeRole> Roles { get; } = new();

    [JsonIgnore]
    public Employee? Supervisor { get; private set; }

    [JsonIgnore]
    public List<Employee> Subordinates { get; private set; } = new();

    public Employee(
        string firstName,
        string lastName,
        DateOnly dateOfBirth,
        DateOnly hiringDate,
        string phoneNumber,
        EmploymentContract contract)
        : base(firstName, lastName, dateOfBirth)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (hiringDate > today)
            throw new ArgumentException("Hiring date cannot be in the future.", nameof(hiringDate));

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.", nameof(phoneNumber));

        Contract = contract ?? throw new ArgumentNullException(nameof(contract));

        HiringDate = hiringDate;
        PhoneNumber = phoneNumber;

        All.Add(this);
    }

    public void ChangeContract(EmploymentContract newContract)
    {
        Contract = newContract ?? throw new ArgumentNullException(nameof(newContract));
    }

    public void AddRole(EmployeeRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (!Roles.Contains(role))
        {
            Roles.Add(role);
        }
    }

    public void RemoveRole(EmployeeRole role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        Roles.Remove(role);
    }

    public void AddSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (ReferenceEquals(employee, this))
            throw new InvalidOperationException("Employee cannot supervise themselves.");

        if (!Subordinates.Contains(employee))
        {
            Subordinates.Add(employee);
            employee.Supervisor = this;
        }
    }

    public void RemoveSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (Subordinates.Remove(employee) && employee.Supervisor == this)
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
        if (!supervisor.Subordinates.Contains(this))
            supervisor.Subordinates.Add(this);
    }

    public void RemoveSupervisor()
    {
        if (Supervisor == null)
            throw new InvalidOperationException("This employee does not have a supervisor.");

        Supervisor.Subordinates.Remove(this);
        Supervisor = null;
    }

    public static void SaveToFile(string filePath)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        var json = JsonSerializer.Serialize(All, options);
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
            All.Clear();
            All.AddRange(employees);
        }
    }
}
