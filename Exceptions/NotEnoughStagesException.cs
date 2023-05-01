namespace JWT_Implementation.Exceptions;

public class NotEnoughStagesException : Exception
{
    public NotEnoughStagesException()
    {
        
    }

    public NotEnoughStagesException(string message) : base(message)
    {
        
    }
}