namespace JWT_Implementation.Exceptions;

public class TicketStageNotFoundException : Exception
{
    public TicketStageNotFoundException()
    {
        
    }

    public TicketStageNotFoundException(string message) : base(message)
    {
        
    }
}