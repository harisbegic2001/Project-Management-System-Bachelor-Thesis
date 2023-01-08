namespace JWT_Implementation.Exceptions;

public class TicketPriorityDoesNotExistException : Exception
{

    public TicketPriorityDoesNotExistException(string message) : base(message)
    {
        
    }
    
    public TicketPriorityDoesNotExistException()
    {
        
    }
}