using System.Reflection;
using Cinema.Core.models.customers;
using Cinema.Core.models.sales;
using Cinema.Core.models.sessions;
using Cinema.Core.models.roles;
using Cinema.Core.models.contract;

namespace Cinema.Core.Tests.Models
{
    [TestFixture]
    public class CinemaTests
    {

        [SetUp]
        public void Setup()
        {
            // prevent data bleeding between tests.
            ClearAllExtents();
        }

        [TearDown]
        public void Cleanup()
        {
            var files = new[] { "customers.json", "sessions.json", "orders.json", "employees.json" };
            foreach (var f in files)
            {
                if (File.Exists(f)) File.Delete(f);
            }
        }

        private void ClearAllExtents()
        {
            // Helper to clear static lists via Reflection
            ClearStaticList<Customer>("_all");
            ClearStaticList<Employee>("_all");
            ClearStaticList<Session>("All"); 
            ClearStaticList<Ticket>("All");  
            ClearStaticList<Seat>("All");    
            ClearStaticList<Order>("_all");
            ClearStaticList<Movie>("All");
            ClearStaticList<Promotion>("_all");
        }

        private void ClearStaticList<T>(string fieldName)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var prop = type.GetProperty(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            object? listObject = null;

            if (field != null) listObject = field.GetValue(null);
            else if (prop != null) listObject = prop.GetValue(null);

            if (listObject != null && listObject is System.Collections.IList list)
            {
                list.Clear();
            }
        }

        // ==================================================================
        // 1. CONTRACT TESTS (Logic Validations)
        // ==================================================================

        [Test]
        public void FullTimeContract_ValidData_ShouldCreate()
        {
            var contract = new FullTimeContract(2500.00m, true);
            Assert.That(contract.Salary, Is.EqualTo(2500.00m));
            Assert.That(contract.HasBenefitsPlan, Is.True);
        }

        [Test]
        public void FullTimeContract_SalaryBelowMin_ShouldThrow()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new FullTimeContract(400m, true));
            Assert.That(ex.Message, Does.Contain($"Salary must be at least"));
        }

        [Test]
        public void FullTimeContract_SalaryAboveMax_ShouldThrow()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new FullTimeContract(4000m, true));
            Assert.That(ex.Message, Does.Contain($"Salary cannot exceed"));
        }

        [Test]
        public void PartTimeContract_HourlyRateInvalid_ShouldThrow()
        {
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => new PartTimeContract(4.00m, 20));
            Assert.That(ex.Message, Does.Contain("Hourly rate must be between"));
        }

        // ==================================================================
        // 2. CUSTOMER & PERSON TESTS (Validation & Hashing)
        // ==================================================================

        [Test]
        public void Customer_Create_ShouldHashPasswordAndAddToExtent()
        {
            string rawPass = "SecurePass123";
            var c = new Customer("Jane", "Smith", new DateOnly(1995, 5, 15), "jane@test.com", rawPass);

            Assert.That(c.HashPassword, Is.Not.EqualTo(rawPass)); // Hashing check
            Assert.That(c.HashPassword.Length, Is.GreaterThan(10));

            // Extent Check (Requirement 4)
            Assert.That(Customer.All, Contains.Item(c));
        }

        [Test]
        public void Customer_InvalidEmail_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1990, 1, 1), "invalid-email", "Pass123"));
        }

        [Test]
        public void Customer_EmptyPassword_ShouldThrow()
        {
            Assert.Throws<ArgumentException>(() =>
                new Customer("Jane", "Smith", new DateOnly(1990, 1, 1), "a@b.com", ""));
        }

        [Test]
        public void Person_AgeCalculation_ShouldBeCorrect()
        {
            // Logic from old PersonTests
            var birthYear = DateTime.Now.Year - 25;
            var p = new Customer("A", "B", new DateOnly(birthYear, 1, 1), "a@b.com", "Pass123");

            // Note: Depends on current day of year vs birth day, but generally:
            Assert.That(p.Age, Is.EqualTo(25).Or.EqualTo(24));
        }

        [Test]
        public void Person_FutureBirthDate_ShouldThrow()
        {
            var future = DateOnly.FromDateTime(DateTime.Now.AddDays(1));
            Assert.Throws<ArgumentException>(() =>
                new Customer("A", "B", future, "a@b.com", "Pass123"));
        }

        // ==================================================================
        // 3. EMPLOYEE TESTS (Roles & Composite Pattern)
        // ==================================================================

        [Test]
        public void Employee_AddRole_ShouldWork()
        {
            var emp = new Employee("John", "Doe", new DateOnly(1990, 1, 1), new DateOnly(2022, 1, 1), "555-5555",
                new FullTimeContract(2000m, true));

            var role = new CashierRole("login1", "Pass123!");
            emp.AddRole(role);

            Assert.That(emp.Roles, Contains.Item(role));
        }

        [Test]
        public void Employee_AddSubordinate_ShouldLinkBiDirectionally()
        {
            var boss = new Employee("Boss", "Man", new DateOnly(1980, 1, 1), new DateOnly(2010, 1, 1), "111", new FullTimeContract(3000m, true));
            var worker = new Employee("Work", "Man", new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "222", new FullTimeContract(2000m, true));

            boss.AddSubordinate(worker);

            Assert.That(boss.Subordinates, Contains.Item(worker));
            Assert.That(worker.Supervisor, Is.EqualTo(boss));
        }

        [Test]
        public void Employee_SelfSupervision_ShouldThrow()
        {
            var emp = new Employee("A", "B", new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "1", new FullTimeContract(2000m, true));
            Assert.Throws<InvalidOperationException>(() => emp.AddSubordinate(emp));
        }

        // ==================================================================
        // 4. ORDER TESTS (XOR Logic & Business Rules)
        // ==================================================================

        [Test]
        public void Order_Online_MustHaveCustomer_AndNoCashier()
        {
            var ticket = CreateDummyTicket();

            // Valid Online
            var c = new Customer("C", "Cust", new DateOnly(1990, 1, 1), "c@c.com", "Pass123");
            var order = new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, new List<Ticket> { ticket }, customer: c);
            Assert.That(order.Id, Is.GreaterThan(0));

            // Invalid: Online but with Cashier
            var cashier = CreateDummyCashier();
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, new List<Ticket> { ticket }, customer: c, cashier: cashier));
            Assert.That(ex.Message, Does.Contain("cannot have a cashier"));
        }

        [Test]
        public void Order_BoxOffice_MustHaveCashier()
        {
            var ticket = CreateDummyTicket();

            // Invalid: BoxOffice with no cashier
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, new List<Ticket> { ticket }));
            Assert.That(ex.Message, Does.Contain("must have a cashier"));
        }

        [Test]
        public void Order_BoxOffice_EmployeeMustHaveCashierRole()
        {
            var ticket = CreateDummyTicket();
            var empNoRole = new Employee("No", "Role", new DateOnly(1990, 1, 1), new DateOnly(2022, 1, 1), "1", new FullTimeContract(2000m, true));

            // Invalid: Employee passed exists, but doesn't have the role
            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.BoxOffice, OrderStatus.Pending, new List<Ticket> { ticket }, cashier: empNoRole));
            Assert.That(ex.Message, Does.Contain("does not have CashierRole"));
        }

        // ==================================================================
        // 5. SALES & SESSION TESTS (Seat, Ticket, Session)
        // ==================================================================

        [Test]
        public void Seat_VipPrice_ShouldApplyMultiplier()
        {
            decimal basePrice = 10.00m;
            var seat = new Seat(SeatType.Vip, basePrice, true);

            // From your model: TicketMultiplier is initialized to 1.8m
            Assert.That(seat.CalculateFinalSeatPrice(), Is.EqualTo(basePrice * 1.8m));
        }

        [Test]
        public void Ticket_FinalPrice_WithPromoAndBonus_ShouldCalculate()
        {
            var session = CreateDummySession();
            var seat = new Seat(SeatType.Normal, 20.00m, true); // Price 20
            var ticket = new Ticket(session, seat);

            // Create Active Promo (-5.00)
            new Promotion(DateTime.Today.AddDays(-1), DateTime.Today.AddDays(1), "TestPromo", 5.00m);

            // Use 3 bonus points (-3.00)
            decimal price = ticket.CalculateFinalPrice(bonusPointsUsed: 3);

            // 20 - 5 - 3 = 12
            Assert.That(price, Is.EqualTo(12.00m));
        }

        [Test]
        public void Ticket_Book_ShouldChangeStatus()
        {
            var ticket = new Ticket(CreateDummySession(), new Seat(SeatType.Normal, 10m, true));

            Assert.That(ticket.IsBooked, Is.False);
            ticket.BookTicket();
            Assert.That(ticket.IsBooked, Is.True);

            Assert.Throws<InvalidOperationException>(() => ticket.BookTicket());
        }

        [Test]
        public void Session_Edit_ShouldUpdateProperties()
        {
            var s = CreateDummySession();
            var newDate = s.StartAt.AddHours(5);
            var newLang = "French";

            Session.EditSession(s, newDate, newLang);

            Assert.That(s.StartAt, Is.EqualTo(newDate));
            Assert.That(s.Language, Is.EqualTo(newLang));
        }

        // ==================================================================
        // 6. PERSISTENCE TESTS (Requirement: JSON Persistence)
        // ==================================================================

        [Test]
        public void Persistence_Customer_ShouldSaveAndLoad()
        {
            // 1. Create Data
            new Customer("Alice", "Save", new DateOnly(1990, 1, 1), "alice@save.com", "Pass123");
            new Customer("Bob", "Load", new DateOnly(1992, 2, 2), "bob@save.com", "Pass123");
            Assert.That(Customer.All.Count, Is.EqualTo(2));

            // 2. Save
            string file = "customers.json";
            Customer.SaveToFile(file);

            // 3. Clear Memory
            ClearAllExtents();
            Assert.That(Customer.All.Count, Is.EqualTo(0));

            // 4. Load
            Customer.LoadFromFile(file);

            // 5. Verify
            Assert.That(Customer.All.Count, Is.EqualTo(2));
            var alice = Customer.All.FirstOrDefault(c => c.Email == "alice@save.com");
            Assert.That(alice, Is.Not.Null);
            Assert.That(alice.FirstName, Is.EqualTo("Alice"));
        }

        [Test]
        public void Persistence_Session_ShouldPreserveReferences()
        {
            var session = CreateDummySession();
            string file = "sessions.json";

            Session.SaveToFile(file);
            ClearAllExtents();

            Session.LoadFromFile(file);

            Assert.That(Session.All.Count, Is.EqualTo(1));
            Assert.That(Session.All[0].Language, Is.EqualTo("English"));
        }

        // ==================================================================
        // HELPERS
        // ==================================================================

        private Session CreateDummySession()
        {
            var hall = new Hall("H1");
            var movie = new Movie("M1", TimeSpan.FromHours(2), new[] { "G" });
            return new Session(hall, movie, DateTime.Now.AddDays(1), "English");
        }

        private Ticket CreateDummyTicket()
        {
            return new Ticket(CreateDummySession(), new Seat(SeatType.Normal, 10m, true));
        }

        private Employee CreateDummyCashier()
        {
            var emp = new Employee("Cash", "Ier", new DateOnly(1990, 1, 1), new DateOnly(2020, 1, 1), "123", new FullTimeContract(2000m, true));
            emp.AddRole(new CashierRole("login", "Pass123!"));
            return emp;
        }
    }
}