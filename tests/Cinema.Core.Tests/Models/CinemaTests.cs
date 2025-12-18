using System.Collections;
using System.Reflection;
using Cinema.Core.models.customers;
using Cinema.Core.models.sales;
using Cinema.Core.models.sessions;
using Cinema.Core.models.roles;
using Cinema.Core.models.operations;

namespace Cinema.Core.Tests.Models
{
    [TestFixture]
    public class CinemaTests
    {
        [SetUp]
        public void Setup()
        {
            ClearAllExtents();
        }

        [TearDown]
        public void Cleanup()
        {
            var files = new[] { "customers.json", "sessions.json", "orders.json", "employees.json", "halls.json", "tickets.json", "shifts.json", "equipment.json" };
            foreach (var f in files)
            {
                if (File.Exists(f)) File.Delete(f);
            }
        }

        // ==================================================================
        // HELPERS (Reflection for Extent Clearing)
        // ==================================================================

        private void ClearAllExtents()
        {
            ClearStaticList<Customer>("_all");
            ClearStaticList<Employee>("_all");
            ClearStaticList<Session>("_all");
            ClearStaticList<Ticket>("_all");
            ClearStaticList<Seat>("All");
            ClearStaticList<Order>("_all");
            ClearStaticProperty<Movie>("All");
            ClearStaticList<Promotion>("_all");
            ClearStaticList<Review>("_all");
            ClearStaticList<Shift>("_all");
            ClearStaticList<Equipment>("_all");
            ClearStaticList<Hall>("_all");
        }

        private void ClearStaticList<T>(string memberName)
        {
            var type = typeof(T);
            var field = type.GetField(memberName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (field != null)
            {
                if (field.GetValue(null) is IList list) list.Clear();
                return;
            }

            var prop = type.GetProperty(memberName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop != null)
            {
                if (prop.GetValue(null) is IList list) list.Clear();
            }
        }

        private void ClearStaticProperty<T>(string propName)
        {
            var type = typeof(T);
            var prop = type.GetProperty(propName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (prop != null && prop.GetValue(null) is IList list)
            {
                list.Clear();
            }
        }

        private List<T> GetStaticList<T>(string fieldName)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var prop = type.GetProperty(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            object? val = null;
            if (field != null) val = field.GetValue(null);
            else if (prop != null) val = prop.GetValue(null);

            return (List<T>)val!;
        }

        private Order CreateOrderWithTickets(int ticketCount, Session session, SeatType seatType)
        {
            // BoxOffice order requires a Cashier
            var cashier = CreateDummyCashierRole();
            var order = new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, cashier: cashier);

            for (int i = 0; i < ticketCount; i++)
            {
                var seat = new Seat(seatType, 10m, true);
                new Ticket(session, seat, order);
            }

            return order;
        }

        private Session CreateDummySession()
        {
            var hall = new Hall("H1");
            var movie = new Movie("Matrix", TimeSpan.FromHours(2), new[] { "SciFi" });
            return new Session(hall, movie, DateTime.Now.AddDays(1), "English");
        }

        private Employee CreateDummyEmployee(string firstName, ContractType type = ContractType.FullTimeContract)
        {
            decimal? salary = type == ContractType.FullTimeContract ? 2000m : null;
            bool? benefits = type == ContractType.FullTimeContract ? true : null;
            decimal? hourly = type == ContractType.PartTimeContract ? 20m : null;
            int? hours = type == ContractType.PartTimeContract ? 20 : null;

            return new Employee(
                firstName,
                "Doe",
                new DateOnly(1990, 1, 1),
                new DateOnly(2023, 1, 1),
                "123456",
                type,
                salary,
                benefits,
                hourly,
                hours
            );
        }

        private TechnicianRole CreateDummyTechnician(string degree)
        {
            var emp = CreateDummyEmployee("TechGuy");
            return new TechnicianRole(emp, degree, true);
        }

        private CashierRole CreateDummyCashierRole()
        {
            var emp = CreateDummyEmployee("CashierEmp");
            return new CashierRole(emp, "POS1", "Pass123!");
        }

        // ==================================================================
        // 1. INHERITANCE: FLATTENING (Employee Contracts)
        // ==================================================================

        [Test]
        public void Flattening_FullTimeEmployee_ShouldHaveSalary_AndNoHourlyRate()
        {
            var emp = new Employee("John", "Full", new DateOnly(1990, 1, 1), DateOnly.FromDateTime(DateTime.Today), "111",
                ContractType.FullTimeContract, salary: 2500m, hasBenefitsPlan: true);

            Assert.That(emp.ContractType, Is.EqualTo(ContractType.FullTimeContract));
            Assert.That(emp.Salary, Is.EqualTo(2500m));
            Assert.That(emp.HasBenefitsPlan, Is.True);
            Assert.That(emp.HourlyRate, Is.Null);
            Assert.That(emp.MaxWeekHours, Is.Null);
        }

        [Test]
        public void Flattening_PartTimeEmployee_ShouldHaveHourlyRate_AndNoSalary()
        {
            var emp = new Employee("Jane", "Part", new DateOnly(1995, 1, 1), DateOnly.FromDateTime(DateTime.Today), "222",
                ContractType.PartTimeContract, hourlyRate: 15.5m, maxWeekHours: 20);

            Assert.That(emp.ContractType, Is.EqualTo(ContractType.PartTimeContract));
            Assert.That(emp.HourlyRate, Is.EqualTo(15.5m));
            Assert.That(emp.MaxWeekHours, Is.EqualTo(20));
            Assert.That(emp.Salary, Is.Null);
            Assert.That(emp.HasBenefitsPlan, Is.Null);
        }

        [Test]
        public void Flattening_Constraint_SettingHourlyRateOnFullTime_ShouldThrow()
        {
            var emp = CreateDummyEmployee("FullTimer", ContractType.FullTimeContract);

            var ex = Assert.Throws<InvalidOperationException>(() => emp.HourlyRate = 20m);
            Assert.That(ex.Message, Does.Contain("Full-time employees cannot be assigned an Hourly Rate"));
        }

        [Test]
        public void Flattening_Constraint_SettingSalaryOnPartTime_ShouldThrow()
        {
            var emp = CreateDummyEmployee("PartTimer", ContractType.PartTimeContract);

            var ex = Assert.Throws<InvalidOperationException>(() => emp.Salary = 3000m);
            Assert.That(ex.Message, Does.Contain("Part-time employees cannot be assigned a Salary"));
        }

        // ==================================================================
        // 2. INHERITANCE: COMPOSITION (Employee Roles)
        // ==================================================================

        [Test]
        public void Composition_AddTechnicianRole_ShouldLinkBidirectionally()
        {
            var emp = CreateDummyEmployee("TechMaster");
            var role = new TechnicianRole(emp, "Sound Master", true);

            Assert.That(emp.TechnicianRole, Is.EqualTo(role));
            Assert.That(role.Employee, Is.EqualTo(emp));
        }

        [Test]
        public void Composition_AddDuplicateRole_ShouldThrow()
        {
            var emp = CreateDummyEmployee("MultiRole");
            var role1 = new TechnicianRole(emp, "Level 1", true);

            var ex = Assert.Throws<InvalidOperationException>(() => new TechnicianRole(emp, "Level 2", false));
            Assert.That(ex.Message, Does.Contain("Employee already has Technician role"));
        }

        [Test]
        public void Composition_DeleteEmployee_ShouldCascadeDeleteRoles()
        {
            var emp = CreateDummyEmployee("ToBeDeleted");
            var techRole = new TechnicianRole(emp, "Degree", true);
            var cleanerRole = new CleanerRole(emp, true, DateOnly.FromDateTime(DateTime.Now.AddMonths(-1)));

            Assert.That(emp.TechnicianRole, Is.Not.Null);
            Assert.That(emp.CleanerRole, Is.Not.Null);

            Employee.DeleteEmployee(emp);

            Assert.That(Employee.All, Does.Not.Contain(emp));
            Assert.That(emp.TechnicianRole, Is.Null);
            Assert.That(emp.CleanerRole, Is.Null);
        }

        [Test]
        public void Composition_RoleMustBelongToSpecificEmployee()
        {
            var emp1 = CreateDummyEmployee("Emp1");
            var emp2 = CreateDummyEmployee("Emp2");
            var roleForEmp1 = new TechnicianRole(emp1, "Degree", true);

            // Attempt to force add emp1's role to emp2
            var ex = Assert.Throws<InvalidOperationException>(() => emp2.AddTechnicianRole(roleForEmp1));
            Assert.That(ex.Message, Does.Contain("role belongs to another employee"));
        }

        // ==================================================================
        // 3. BASIC ASSOCIATION & MULTIPLICITY
        // ==================================================================

        [Test]
        public void Association_SessionTechnician_ShouldLinkReverseConnection()
        {
            var session = CreateDummySession();
            var tech = CreateDummyTechnician("Sound Engineer");

            session.AddTechnician(tech);

            Assert.That(session.Technicians, Contains.Item(tech));
            Assert.That(tech.Sessions, Contains.Item(session));
        }

        [Test]
        public void Multiplicity_SessionMustHaveOneTechnician_RemoveShouldThrow()
        {
            var session = CreateDummySession();
            var tech1 = CreateDummyTechnician("Tech One");
            var tech2 = CreateDummyTechnician("Tech Two");

            session.AddTechnician(tech1);
            session.AddTechnician(tech2);

            session.RemoveTechnician(tech1);
            Assert.That(session.Technicians, Does.Not.Contain(tech1));

            var cleanSession = CreateDummySession();

            var realTech = CreateDummyTechnician("Real");
            cleanSession.AddTechnician(realTech);

            cleanSession.RemoveTechnician(realTech);

            var dummy = cleanSession.Technicians.First();
            var ex = Assert.Throws<InvalidOperationException>(() => cleanSession.RemoveTechnician(dummy));
            Assert.That(ex.Message, Does.Contain("at least one technician"));
        }

        // ==================================================================
        // 4. COMPOSITION ASSOCIATION (Ticket-Order)
        // ==================================================================

        [Test]
        public void Composition_TicketCannotExistWithoutOrder()
        {
            var session = CreateDummySession();
            var seat = new Seat(SeatType.Normal, 10m, true);
            Assert.Throws<ArgumentNullException>(() => new Ticket(session, seat, null!));
        }

        [Test]
        public void Composition_DeletePart_ShouldRemoveFromWhole()
        {
            var session = CreateDummySession();
            var order = CreateOrderWithTickets(2, session, SeatType.Normal);
            var ticketToRemove = order.Tickets[0];

            ticketToRemove.DeletePart();

            Assert.That(order.Tickets, Does.Not.Contain(ticketToRemove));
            Assert.That(Ticket.All, Does.Not.Contain(ticketToRemove));
            Assert.That(order.Tickets.Count, Is.EqualTo(1));
        }

        // ==================================================================
        // 5. AGGREGATION ASSOCIATION
        // ==================================================================

        [Test]
        public void Aggregation_EquipmentLinkedToHall_ButHallExistsIndependently()
        {
            var hall = new Hall("IMAX Hall");
            var equipment = new Equipment(EquipmentType.Audio, DateTime.Now, hall);

            Assert.That(hall.Equipment, Contains.Item(equipment));
            Assert.That(equipment.Hall, Is.EqualTo(hall));

            var equipment2 = new Equipment(EquipmentType.Projection, DateTime.Now, hall);
            Assert.That(hall.Equipment.Count, Is.EqualTo(2));

            hall.RemoveEquipment(equipment);
            Assert.That(hall.Equipment, Does.Not.Contain(equipment));
            Assert.That(hall.Name, Is.EqualTo("IMAX Hall"));
        }

        // ==================================================================
        // 6. REFLEX ASSOCIATION
        // ==================================================================

        [Test]
        public void Reflex_SetSupervisor_ShouldUpdateSubordinatesList()
        {
            var supervisor = CreateDummyEmployee("Boss");
            var worker = CreateDummyEmployee("Worker");

            worker.SetSupervisor(supervisor);

            Assert.That(worker.Supervisor, Is.EqualTo(supervisor));
            Assert.That(supervisor.Subordinates, Contains.Item(worker));
        }

        [Test]
        public void Reflex_SelfSupervision_ShouldThrow()
        {
            var emp = CreateDummyEmployee("LoneWolf");
            Assert.Throws<InvalidOperationException>(() => emp.SetSupervisor(emp));
            Assert.Throws<InvalidOperationException>(() => emp.AddSubordinate(emp));
        }

        // ==================================================================
        // 7. QUALIFIED ASSOCIATION
        // ==================================================================

        [Test]
        public void QualifiedAssociation_AddSeat_ShouldBeRetrievableByNumber()
        {
            var hall = new Hall("Standard");
            var seat = new Seat(SeatType.Normal, 10m, true);

            hall.AddSeat(seat);

            var retrieved = hall.GetSeat(seat.Id);
            Assert.That(retrieved, Is.EqualTo(seat));
        }

        // ==================================================================
        // 8. XOR & BUSINESS LOGIC
        // ==================================================================

        [Test]
        public void XOR_Order_OnlineMustHaveCustomer()
        {
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, customer: null, cashier: null));

            Assert.That(ex.Message, Does.Contain("Online order must have a customer"));
        }

        [Test]
        public void XOR_Order_OnlineCannotHaveCashier()
        {
            var cashier = CreateDummyCashierRole();
            var customer = new Customer("Test", "User", new DateOnly(2000, 1, 1), "t@test.com", "123456");

            var order = new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, customer: customer);

            var ex = Assert.Throws<ArgumentException>(() => order.SetCashier(cashier));
            Assert.That(ex.Message, Does.Contain("Online order cannot have a cashier"));
        }

        // ==================================================================
        // 9. PERSISTENCE
        // ==================================================================

        [Test]
        public void Persistence_FullFlow_SaveAndLoad_Hall()
        {
            var hall = new Hall("PersistenceHall");
            hall.AddSeat(new Seat(SeatType.Normal, 10, true));

            Hall.SaveToFile("halls.json");
            ClearAllExtents();
            Hall.LoadFromFile("halls.json");

            var loadedHalls = GetStaticList<Hall>("_all");

            Assert.That(loadedHalls.Count, Is.EqualTo(1));
            Assert.That(loadedHalls[0].Name, Is.EqualTo("PersistenceHall"));
        }

        [Test]
        public void Persistence_FullFlow_SaveAndLoad_EmployeeFlattened()
        {
            var emp = new Employee("SaveMe", "Jones", new DateOnly(1990, 1, 1), DateOnly.FromDateTime(DateTime.Today),
                "999", ContractType.PartTimeContract, hourlyRate: 50m, maxWeekHours: 25);

            Employee.SaveToFile("employees.json");
            ClearAllExtents();
            Employee.LoadFromFile("employees.json");

            var loadedEmps = GetStaticList<Employee>("_all");
            var loadedEmp = loadedEmps.FirstOrDefault(e => e.FirstName == "SaveMe");

            Assert.That(loadedEmp, Is.Not.Null);
            Assert.That(loadedEmp!.ContractType, Is.EqualTo(ContractType.PartTimeContract));
            Assert.That(loadedEmp.HourlyRate, Is.EqualTo(50m));
            Assert.That(loadedEmp.Salary, Is.Null);
        }
    }
}