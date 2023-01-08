using System.ComponentModel.DataAnnotations;

namespace JWT_Implementation.Entities;

public class User
{
    [Key]
    public int Id { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? Occupation { get; set; }
    
    public string? Username { get; set; }

    public byte[]? PasswordHash { get; set; }

    public byte[]? PasswordSalt { get; set; }

    public Role AppRole { get; set; }

    public string? Email { get; set; }
    
}