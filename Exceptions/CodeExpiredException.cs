namespace JWT_Implementation.Exceptions;

public class CodeExpiredException : Exception
{

    public CodeExpiredException()
    {
        
    }

    public CodeExpiredException(string message) : base(message)
    {
        
    }
}