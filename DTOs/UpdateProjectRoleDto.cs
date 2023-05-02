using JWT_Implementation.Entities;

namespace JWT_Implementation.DTOs;

public class UpdateProjectRoleDto
{
    public int UserId { get; set; }

    public Role RoleToUpdate { get; set; }
}