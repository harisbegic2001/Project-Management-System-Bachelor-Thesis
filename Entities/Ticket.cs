using System.ComponentModel.DataAnnotations;

namespace JWT_Implementation.Entities;

public class Ticket
{
    [Key]
    public int Id { get; set; }
    
    public string? TicketName { get; set; }

    public string? TicketKey { get; set; }

    public string? TicketDescription { get; set; }

    public string? TicketPriority { get; set; }

    public string? TicketTask { get; set; }

    public string? TicketReporter { get; set; }

    public int UserId { get; set; }
    
    public int ProjectId { get; set; }

}