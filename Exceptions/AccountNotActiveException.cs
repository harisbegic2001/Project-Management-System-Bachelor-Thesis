namespace JWT_Implementation.Exceptions;

public class AccountNotActiveException : Exception
{

    public AccountNotActiveException()
    {
        
    }

    public AccountNotActiveException(string message) : base(message)
    {
        
    }
}