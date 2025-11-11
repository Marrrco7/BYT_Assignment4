using Cinema.Core.Models;

namespace Cinema.Core;

public class Class1
{

    public static void Main()
    {
        var fullTimeContract = new FullTimeContract(2500m,  true);

        var employee = new Employee(
            firstName: "Ivan",
            lastName: "Petrov",
            dateOfBirth: new DateOnly(1990, 3, 15),
            hiringDate: DateOnly.FromDateTime(DateTime.Now),
            phoneNumber: "+375291234567",
            contract: fullTimeContract
        );

        var cashierRole = new CashierRole("ivan_cash", "Secure123");

        employee.AddRole(cashierRole);

        Console.WriteLine($"Employee: {employee.FirstName} {employee.LastName}");
        Console.WriteLine($"Contract: {employee.Contract.GetType().Name}");
        Console.WriteLine($"Roles ({employee.Roles.Count}):");

        foreach (var role in employee.Roles)
            Console.WriteLine($" - {role.GetType().Name}");

        Console.WriteLine();
        Console.WriteLine($"Phone: {employee.PhoneNumber}");
        Console.WriteLine($"Hiring date: {employee.HiringDate}");
    }
}


