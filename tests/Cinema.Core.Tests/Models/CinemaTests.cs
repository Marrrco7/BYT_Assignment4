using System.Collections;
using System.Reflection;
using Cinema.Core.models.customers;
using Cinema.Core.models.sales;
using Cinema.Core.models.sessions;
using Cinema.Core.models.roles;
using Cinema.Core.models.contract;
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
            var files = new[] { "customers.json", "sessions.json", "orders.json", "employees.json", "halls.json" };
            foreach (var f in files)
            {
                if (File.Exists(f)) File.Delete(f);
            }
        }

        // ==================================================================
        // HELPERS (Reflection)
        // ==================================================================

        private void ClearAllExtents()
        {
            ClearStaticList<Customer>("_all");
            ClearStaticList<Employee>("_all");
            ClearStaticList<Session>("_all");
            ClearStaticList<Ticket>("All");
            ClearStaticList<Seat>("All");
            ClearStaticList<Order>("_all");
            ClearStaticList<Movie>("All");
            ClearStaticList<Promotion>("_all");
            ClearStaticList<Review>("_all");
            ClearStaticList<Shift>("_all");
            ClearStaticList<Equipment>("_all");
            ClearStaticList<Hall>("_all");
        }

        private void ClearStaticList<T>(string fieldName)
        {
            var type = typeof(T);
            var field = type.GetField(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            var prop = type.GetProperty(fieldName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

            object? listObject = null;
            if (field != null) listObject = field.GetValue(null);
            else if (prop != null) listObject = prop.GetValue(null);

            if (listObject is IList list)
            {
                list.Clear();
            }
        }

        private void InvokeInternalStaticMethod(Type type, string methodName, object[] parameters)
        {
            var method = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            if (method == null)
                throw new MissingMethodException($"{type.Name} does not have a static method named {methodName}");

            try
            {
                method.Invoke(null, parameters);
            }
            catch (TargetInvocationException ex)
            {
                if (ex.InnerException != null) throw ex.InnerException;
                throw;
            }
        }

        private void SetPrivateProperty(object obj, string propName, object? value)
        {
            var prop = obj.GetType().GetProperty(propName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (prop == null) throw new MissingMemberException(obj.GetType().Name, propName);
            prop.SetValue(obj, value);
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

        // Helper to construct an Order with Tickets avoiding circular dependency
        private Order CreateOrderWithTickets(int ticketCount, Session session, SeatType seatType)
        {
            var order = new Order();
            var tickets = new List<Ticket>();

            for (int i = 0; i < ticketCount; i++)
            {
                var seat = new Seat(seatType, 10m, true);
                var t = new Ticket(session, seat, order);
                tickets.Add(t);
            }

            var ticketsField = typeof(Order).GetField("_tickets", BindingFlags.Instance | BindingFlags.NonPublic);
            if (ticketsField != null)
            {
                var orderTickets = (List<Ticket>)ticketsField.GetValue(order)!;
                orderTickets.AddRange(tickets);
            }

            return order;
        }

        // ==================================================================
        // 1. BASIC ASSOCIATION & MULTIPLICITY
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
        public void Association_TechnicianSession_ShouldLinkReverseConnection()
        {
            var session = CreateDummySession();
            var tech = CreateDummyTechnician("Projectionist");

            tech.AssignToSession(session);

            Assert.That(tech.Sessions, Contains.Item(session));
            Assert.That(session.Technicians, Contains.Item(tech));
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
            Assert.That(session.Technicians.Count, Is.EqualTo(1));

            var ex = Assert.Throws<InvalidOperationException>(() => session.RemoveTechnician(tech2));
            Assert.That(ex.Message, Does.Contain("must have at least one technician"));
        }

        // ==================================================================
        // 2. COMPOSITION ASSOCIATION
        // ==================================================================

        [Test]
        public void Composition_TicketCannotExistWithoutOrder()
        {
            var session = CreateDummySession();
            var seat = new Seat(SeatType.Normal, 10m, true);
            Assert.Throws<ArgumentNullException>(() => new Ticket(session, seat, null!));
        }

        [Test]
        public void Composition_DeleteOrderPart_ShouldRemoveFromWhole()
        {
            var session = CreateDummySession();
            var order = CreateOrderWithTickets(2, session, SeatType.Normal);
            var ticketToRemove = order.Tickets[0];

            InvokeInternalStaticMethod(typeof(Ticket), "DeleteOrderPart", new object[] { ticketToRemove });

            Assert.That(order.Tickets, Does.Not.Contain(ticketToRemove));
            Assert.That(Ticket.All, Does.Not.Contain(ticketToRemove));
            Assert.That(order.Tickets.Count, Is.EqualTo(1));
        }

        [Test]
        public void Composition_DeleteOrder_ShouldCascadeDeleteTickets()
        {
            var session = CreateDummySession();
            var order = CreateOrderWithTickets(3, session, SeatType.Normal);
            var tickets = order.Tickets.ToList();

            Order.DeleteOrder(order);

            Assert.That(Order.All, Does.Not.Contain(order));
            foreach (var t in tickets)
            {
                Assert.That(Ticket.All, Does.Not.Contain(t), "Ticket should be deleted when Order is deleted");
            }
        }

        // ==================================================================
        // 3. AGGREGATION ASSOCIATION
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
        }

        // ==================================================================
        // 4. REFLEX ASSOCIATION
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
        public void Reflex_ChangeSupervisor_ShouldUpdateLinks()
        {
            var boss1 = CreateDummyEmployee("Boss1");
            var boss2 = CreateDummyEmployee("Boss2");
            var worker = CreateDummyEmployee("Worker");

            worker.SetSupervisor(boss1);
            worker.SetSupervisor(boss2);

            Assert.That(worker.Supervisor, Is.EqualTo(boss2));
            Assert.That(boss1.Subordinates, Does.Not.Contain(worker));
            Assert.That(boss2.Subordinates, Contains.Item(worker));
        }

        [Test]
        public void Reflex_SelfSupervision_ShouldThrow()
        {
            var emp = CreateDummyEmployee("LoneWolf");
            Assert.Throws<InvalidOperationException>(() => emp.SetSupervisor(emp));
            Assert.Throws<InvalidOperationException>(() => emp.AddSubordinate(emp));
        }

        // ==================================================================
        // 5. QUALIFIED ASSOCIATION
        // ==================================================================

        [Test]
        public void QualifiedAssociation_AddSeat_ShouldBeRetrievableByNumber()
        {
            var hall = new Hall("Standard");
            var seat = new Seat(SeatType.Normal, 10m, true);
            int seatNum = 5;

            hall.AddSeat(seatNum, seat);

            var retrieved = hall.GetSeat(seatNum);
            Assert.That(retrieved, Is.EqualTo(seat));
        }

        [Test]
        public void QualifiedAssociation_DuplicateSeatNumber_ShouldThrow()
        {
            var hall = new Hall("Standard");
            var seat1 = new Seat(SeatType.Normal, 10m, true);
            var seat2 = new Seat(SeatType.Vip, 20m, true);

            hall.AddSeat(1, seat1);
            var ex = Assert.Throws<InvalidOperationException>(() => hall.AddSeat(1, seat2));
            Assert.That(ex.Message, Does.Contain("already exists"));
        }

        // ==================================================================
        // 6. ASSOCIATION WITH ATTRIBUTE
        // ==================================================================

        [Test]
        public void AssociationClass_Review_ShouldLinkCustomerAndSession()
        {
            var customer = new Customer("John", "Critic", new DateOnly(1990, 1, 1), "crit@test.com", "Pass123");
            var session = CreateDummySession();

            var review = new Review(5, 4, DateTime.Now, "Great!", customer, session);

            Assert.That(customer.Reviews, Contains.Item(review));
            Assert.That(session.Reviews, Contains.Item(review));
            Assert.That(review.Author, Is.EqualTo(customer));
            Assert.That(review.ReviewedSession, Is.EqualTo(session));
        }

        // ==================================================================
        // 7. XOR & BUSINESS LOGIC
        // ==================================================================

        [Test]
        public void XOR_Order_OnlineMustHaveCustomer()
        {
            var session = CreateDummySession();
            var tempOrder = new Order();
            var ticket = new Ticket(session, new Seat(SeatType.Normal, 10, true), tempOrder);

            // Detach ticket so it can be used in new Order
            SetPrivateProperty(ticket, "Order", null);

            var tickets = new List<Ticket> { ticket };

            var ex = Assert.Throws<ArgumentException>(() =>
                new Order(DateTime.Now, TypeOfOrder.Online, OrderStatus.Pending, tickets, customer: null, cashier: null));

            Assert.That(ex.Message, Does.Contain("Online order must have a customer"));
        }

        [Test]
        public void XOR_Order_OnlineCannotHaveCashier()
        {
            var cashier = CreateDummyCashier();
            var order = new Order();

            SetPrivateProperty(order, "TypeOfOrder", TypeOfOrder.Online);

            // Force validation to proceed to cashier check by adding a customer first
            var customer = new Customer("Test", "User", new DateOnly(2000, 1, 1), "t@test.com", "123456");
            order.SetCustomer(customer);

            var ex = Assert.Throws<ArgumentException>(() => order.Cashier = cashier);
            Assert.That(ex.Message, Does.Contain("Online order cannot have a cashier"));
        }

        // ==================================================================
        // 8. LOGIC & PERSISTENCE
        // ==================================================================

        [Test]
        public void Logic_SeatPrice_VipMultiplier()
        {
            var seat = new Seat(SeatType.Vip, 100m, true);
            Assert.That(seat.CalculateFinalSeatPrice(), Is.EqualTo(180m));
        }

        [Test]
        public void Persistence_FullFlow_SaveAndLoad()
        {
            var hall = new Hall("PersistenceHall");

            var allHalls = GetStaticList<Hall>("_all");
            if (!allHalls.Contains(hall))
            {
                allHalls.Add(hall);
            }

            hall.AddSeat(1, new Seat(SeatType.Normal, 10, true));

            Hall.SaveToFile("halls.json");

            ClearAllExtents();

            Hall.LoadFromFile("halls.json");

            var loadedHalls = GetStaticList<Hall>("_all");

            Assert.That(loadedHalls.Count, Is.EqualTo(1));
            Assert.That(loadedHalls[0].Name, Is.EqualTo("PersistenceHall"));
        }

        // ==================================================================
        // TEST HELPERS
        // ==================================================================

        private Session CreateDummySession()
        {
            var hall = new Hall("H1");
            var movie = new Movie("Matrix", TimeSpan.FromHours(2), new[] { "SciFi" });
            return new Session(hall, movie, DateTime.Now.AddDays(1), "English");
        }

        private Employee CreateDummyEmployee(string firstName)
        {
            var contract = new FullTimeContract(2000m, true);
            return new Employee(firstName, "Doe", new DateOnly(1990, 1, 1),
                new DateOnly(2023, 1, 1), "123456", contract);
        }

        private TechnicianRole CreateDummyTechnician(string degree)
        {
            return new TechnicianRole(degree, true);
        }

        private Employee CreateDummyCashier()
        {
            var emp = CreateDummyEmployee("Cashier");
            emp.AddRole(new CashierRole("POS1", "Pass123!"));
            return emp;
        }
    }
}