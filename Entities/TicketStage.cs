using System.ComponentModel.DataAnnotations;

namespace JWT_Implementation.Entities;

public class TicketStage
{
    [Key]
    public int Id { get; set; }

    public string? StageName { get; set; }

    public int ProjectId { get; set; }
}