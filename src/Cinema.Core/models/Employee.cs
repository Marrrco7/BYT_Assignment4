namespace Cinema.Core.models;

public abstract class Employee : Person
{
    public DateOnly HiringDate { get; private set; }
    public string PhoneNumber { get; private set; }
    
    public Employee? Supervisor { get; private set; }          
    public List<Employee> Subordinates { get; private set; } = new(); 

    public Employee(string firstName, string lastName, DateOnly dateOfBirth,
        DateOnly hiringDate, string phoneNumber)
        : base(firstName, lastName, dateOfBirth)
    {
        if (hiringDate > DateOnly.FromDateTime(DateTime.Now))
            throw new ArgumentException("Hiring date cannot be in the future.");

        if (string.IsNullOrWhiteSpace(phoneNumber))
            throw new ArgumentException("Phone number cannot be empty.");

        HiringDate = hiringDate;
        PhoneNumber = phoneNumber;
    }
    
    public void AddSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (employee == this)
            throw new InvalidOperationException("Employee cannot supervise themselves.");

        if (!Subordinates.Contains(employee))
            Subordinates.Add(employee);
    }
    
    public void RemoveSubordinate(Employee employee)
    {
        if (employee == null)
            throw new ArgumentNullException(nameof(employee));

        if (Subordinates.Remove(employee))
            employee.Supervisor = null;
    }

    public void AssignSupervisor(Employee supervisor)
    {
        if (supervisor == null)
            throw new ArgumentNullException(nameof(supervisor));

        if (supervisor == this)
            throw new InvalidOperationException("Employee cannot be their own supervisor.");

        Supervisor = supervisor;
    }
    
    
    public void RemoveSupervisor()
    {
        if (Supervisor == null)
            throw new InvalidOperationException("This employee does not have a supervisor.");

        Supervisor.Subordinates.Remove(this);
        Supervisor = null;
    }
}
