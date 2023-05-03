using System.Data.SqlClient;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using JWT_Implementation.Constants;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Services.Interfaces;
using JWT_Implementation.TokenService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace JWT_Implementation.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{

    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Register function
    /// </summary>
    /// <returns> Json Web Token and created User</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> RegisterUserAsync(UserDto userDto)
    {
        try
        {
            var registeredUser = await _userService.RegisterUserAsync(userDto);
            return Ok(registeredUser);
        }
        catch (UserAlreadyExistsException e)
        {
            return BadRequest(e.Message);
        }
        catch (InvalidDataException e)
        {
            return BadRequest(e.Message);
        }
    }


    [HttpPost("login")]
    public async Task<ActionResult<ReadUserDto>> LoginUserAsync(LoginDto loginDto)
    {
        try
        {
            var loggedUser = await _userService.LoginUserAsync(loginDto);

            return Ok(loggedUser);
        }
        catch (InvalidUserNameOrPasswordException e)
        {
            return Unauthorized(e.Message);
        }
    }


    [Authorize(Roles = nameof(Role.Admin))]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersAsync()
    {
        
        return Ok( await _userService.GetUsersAsync());
    }

    [HttpPut]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult> UpdateUserRoleAsync(int id, string role)
    {
        try
        {
            await _userService.UpdateUserRoleAsync(id, role);
            return Ok();
        }
        catch (UserNotFoundException e)
        {
            return NotFound(e.Message);
        }
        catch (RoleNotFoundException e)
        {
            return NotFound(e.Message);
        }
    }


    [HttpGet("GetUsersOnProject")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsersOnProjectAsync(int projectId)
    {
           
        var usersOnProject = await _userService.GetAllUsersOnProjectAsync(projectId);
        return Ok(usersOnProject);

   
    }
}