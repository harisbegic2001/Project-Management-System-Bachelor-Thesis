namespace JWT_Implementation.Exceptions;

public class InvalidVerificationCodeException : Exception
{

    public InvalidVerificationCodeException()
    {
        
    }

    public InvalidVerificationCodeException(string message) : base(message)
    {
        
    }
}