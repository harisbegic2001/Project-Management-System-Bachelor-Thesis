namespace JWT_Implementation.DTOs;

public class CreateProjectDto
{
    public string? ProjectName { get; set; }
    
    public string? ProjectKey { get; set; }
    
    public string? ProjectType { get; set; }
    
    public string? ProjectDescription { get; set; }
}