namespace JWT_Implementation.Exceptions;

public class InvalidUserNameOrPasswordException : Exception
{

    public InvalidUserNameOrPasswordException()
    {
        
    }

    public InvalidUserNameOrPasswordException(string message) : base(message)
    {
        
    }
}