namespace JWT_Implementation.DTOs;

public class UpdateTicketDto
{
    public string? TicketName { get; set; }
    
    public string? TicketDescription { get; set; }

    public string? TicketPriority { get; set; }

    public string? TicketType { get; set; }
    
    public int TicketReporterId { get; set; }

    public int TicketStageId { get; set; }
}