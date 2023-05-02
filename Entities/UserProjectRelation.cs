namespace JWT_Implementation.Entities;

public class UserProjectRelation
{
    public int UserId { get; set; }
    
    public int ProjectId { get; set; }

    public Role ProjectRole { get; set; }

}