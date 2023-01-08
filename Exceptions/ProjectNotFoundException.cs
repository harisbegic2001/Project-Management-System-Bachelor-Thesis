namespace JWT_Implementation.Exceptions;

public class ProjectNotFoundException : Exception
{
    public ProjectNotFoundException(string message) : base(message)
    {
        
    }
    
    public ProjectNotFoundException()
    {
        
    }
}