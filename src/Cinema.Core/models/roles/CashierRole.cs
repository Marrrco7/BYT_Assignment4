namespace Cinema.Core.models.roles;

public sealed class CashierRole : EmployeeRole
{
    public string POSLogin { get; }
    public string POSPassword { get; }

    public CashierRole(string posLogin, string posPassword)
    {
        if (string.IsNullOrWhiteSpace(posLogin))
            throw new ArgumentException("POS login cannot be empty.", nameof(posLogin));
        if (posLogin.Length < 3)
            throw new ArgumentException("POS login must be at least 3 characters long.", nameof(posLogin));
        if (!posLogin.All(char.IsLetterOrDigit))
            throw new ArgumentException("POS login must contain only letters or digits.", nameof(posLogin));

        if (string.IsNullOrWhiteSpace(posPassword))
            throw new ArgumentException("POS password cannot be empty.", nameof(posPassword));
        if (posPassword.Length < 6)
            throw new ArgumentException("POS password must be at least 6 characters long.", nameof(posPassword));
        if (!posPassword.Any(char.IsUpper))
            throw new ArgumentException("POS password must contain at least one uppercase letter.", nameof(posPassword));
        if (!posPassword.Any(char.IsDigit))
            throw new ArgumentException("POS password must contain at least one number.", nameof(posPassword));

        POSLogin = posLogin;
        POSPassword = posPassword;
    }
}