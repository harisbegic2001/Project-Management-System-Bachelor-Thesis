using System.ComponentModel.DataAnnotations;

namespace JWT_Implementation.Entities;

public class Comment
{
    [Key]
    public int Id { get; set; }

    public string? CommentSource { get; set; }

    public DateTime DateOfCreation { get; set; }

    public int TicketId { get; set; }
    
    public int CreatorId { get; set; }
}