namespace JWT_Implementation.Exceptions;

public class UserNotOnProjectException : Exception
{
    public UserNotOnProjectException()
    {
        
    }
    
    public UserNotOnProjectException(string message) : base(message)
    {
        
    }
}