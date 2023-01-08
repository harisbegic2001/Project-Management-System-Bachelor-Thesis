using System.ComponentModel.DataAnnotations;

namespace JWT_Implementation.Entities;

public class Project
{
    [Key]
    public int Id { get; set; }
    
    public string? ProjectName { get; set; }
    
    public string? ProjectKey { get; set; }
    
    public string? ProjectType { get; set; }
    
    public string? ProjectDescription { get; set; }

    public string? ProjectColumns { get; set; }
    
}