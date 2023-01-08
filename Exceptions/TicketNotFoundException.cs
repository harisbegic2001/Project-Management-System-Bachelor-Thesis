namespace JWT_Implementation.Exceptions;

public class TicketNotFoundException : Exception
{
    public TicketNotFoundException(string message) : base(message)
    {
        
    }
    
    
    public TicketNotFoundException()
    {
        
    }
}