namespace JWT_Implementation.Exceptions;

public class TicketTaskDoesNotExistException : Exception
{
    
    public TicketTaskDoesNotExistException(string message) : base(message)
    {
        
    }
    
    

    public TicketTaskDoesNotExistException()
    {
        
    }
    
}