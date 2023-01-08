namespace JWT_Implementation.Exceptions;

public class RoleNotFoundException : Exception
{

    public RoleNotFoundException(string message) : base(message)
    {
        
    }
    
    public RoleNotFoundException()
    {
        
    }

}