using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;

namespace JWT_Implementation.Services.Interfaces;

public interface IUserService
{
    Task<ReadUserDto> RegisterUserAsync(UserDto userDto);

    Task<ReadUserDto> LoginUserAsync(LoginDto loginDto);

    Task<IEnumerable<User>> GetUsersAsync();

    Task UpdateUserRoleAsync(int id, string role);

    Task<IEnumerable<User>> GetAllUsersOnProjectAsync(int projectId);

    


}