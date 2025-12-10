using Cinema.Core.models.sales;

namespace Cinema.Core.models.roles;

public sealed class CashierRole : EmployeeRole
{
    private string _posLogin = null!;
    private string _posPassword = null!;

    // --- Reverse connection with Order ---

    private readonly List<Order> _orders = new();
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    // --- Properties ---

    public string POSLogin
    {
        get => _posLogin;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("POS login cannot be empty.", nameof(value));
            if (value.Length < 3)
                throw new ArgumentException("POS login must be at least 3 characters long.", nameof(value));
            if (!value.All(char.IsLetterOrDigit))
                throw new ArgumentException("POS login must contain only letters or digits.", nameof(value));

            _posLogin = value;
        }
    }

    public string POSPassword
    {
        get => _posPassword;
        private set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("POS password cannot be empty.", nameof(value));
            if (value.Length < 6)
                throw new ArgumentException("POS password must be at least 6 characters long.", nameof(value));
            if (!value.Any(char.IsUpper))
                throw new ArgumentException("POS password must contain at least one uppercase letter.", nameof(value));
            if (!value.Any(char.IsDigit))
                throw new ArgumentException("POS password must contain at least one number.", nameof(value));

            _posPassword = value;
        }
    }


    public CashierRole(string posLogin, string posPassword)
    {
        POSLogin = posLogin;
        POSPassword = posPassword;
    }

    // --- Association  Orders ---

    public void AddOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (_orders.Contains(order))
        {
            if (order.Cashier != this)
            {
                order.SetCashier(this);
            }
            return;
        }

        _orders.Add(order);

        if (order.Cashier != this)
        {
            order.SetCashier(this);
        }
    }

    public void RemoveOrder(Order order)
    {
        if (order == null)
            throw new ArgumentNullException(nameof(order));

        if (!_orders.Contains(order))
            return;

        _orders.Remove(order);

        if (order.Cashier == this)
        {
            order.SetCashier(null);
        }
    }
}
