namespace JWT_Implementation.DTOs;

public class ReadTicketDto
{
    public string? TicketName { get; set; }
    
    public string? TicketDescription { get; set; }

    public string? TicketPriority { get; set; }

    public string? TicketTask { get; set; }

    public string? TicketReporter { get; set; }
    
    public string? TicketKey { get; set; }

    public string? TicketProject { get; set; }
}