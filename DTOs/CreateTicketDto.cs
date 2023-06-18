namespace JWT_Implementation.DTOs;

public class CreateTicketDto
{
    public string? TicketName { get; set; }
    
    public string? TicketDescription { get; set; }

    public string? TicketPriority { get; set; }

    public string? TicketType { get; set; }

    public string? AsigneeEmail { get; set; }
    
}