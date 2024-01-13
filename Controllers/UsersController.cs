using System.Data.SqlClient;
using Dapper;
using Google.Apis.Auth;
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
    
    private readonly GoogleSettings _googleSettings;

    private readonly ITokenService _tokenService;

    private readonly ConnectionStrings _connection;

    public UsersController(IUserService userService, IOptions<GoogleSettings> googleOptions, ITokenService tokenService, IOptions<ConnectionStrings> connection)
    {
        _userService = userService;
        _googleSettings = googleOptions.Value;
        _tokenService = tokenService;
        _connection = connection.Value;
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
        catch (AccountNotActiveException)
        {
            return Unauthorized("Account is not activated!");
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


    [HttpGet("GetUsersOnProject/{projectId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsersOnProjectAsync(int projectId)
    {
           
        var usersOnProject = await _userService.GetAllUsersOnProjectAsync(projectId);
        return Ok(usersOnProject);

   
    }

    [HttpPut("activate")]
    [AllowAnonymous]
    public async Task<ActionResult<ReadUserDto>> ActivateUserAsync(AcitvateUserDto acitvateUserDto)
    {
        try
        {
            return Ok(await _userService.ActivateUserAsync(acitvateUserDto));
        }
        catch (CodeExpiredException e)
        {
            return Unauthorized("Your confirmation code has expired, please try again.");
        }
        catch (InvalidVerificationCodeException e)
        {
            return Unauthorized("Invalid Verification code.");
        }
    }
    
    
    [HttpPost("LoginWithGoogle")]
    public async Task<IActionResult> LoginWithGoogle([FromBody] string credential)
    {
        using var connection = CreateSqlConnection();
        
        var settings = new GoogleJsonWebSignature.ValidationSettings()
        {
            Audience = new List<string> { _googleSettings.ClientId! }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(credential, settings);

        var user = await connection.QueryFirstOrDefaultAsync<User>($"SELECT * FROM Users WHERE Email = '{payload.Email}'");

        if (user is not null)
        {
            return Ok(new ReadUserDto
                {
                    Email = user.Email,
                    Token = _tokenService.CreateToken(user),
                    Id = user.Id
                }
                );
        }
        else
        {
            var userToBeAdded = new User
            {
                Id = 0,
                FirstName = payload.Name,
                LastName = null,
                Occupation = null,
                Username = payload.Email,
                PasswordHash = new byte[]
                {
                },
                PasswordSalt = new byte[]
                {
                },
                AppRole = Role.User,
                Email = payload.Email,
                IsActivated = true,
                EmailVerificationCode = null,
                CodeExpirationTime = DateTime.Now,

            };

            await connection.ExecuteAsync("INSERT INTO Users (Firstname, Lastname, Occupation, Username, PasswordHash, PasswordSalt, AppRole, Email, IsActivated, EmailVerificationCode, CodeExpirationTime) " +
                                          "values (@FirstName, @Lastname, @Occupation, @Username, @PasswordHash, @PasswordSalt, @AppRole, @Email,  @IsActivated, @EmailVerificationCode, @CodeExpirationTime)", userToBeAdded);
            
            
            return Ok(new ReadUserDto
                {
                    Email = userToBeAdded.Email,
                    Token = _tokenService.CreateToken(userToBeAdded),
                    Id = userToBeAdded.Id
                }
            );
        }
    }
    
    
    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_connection.DefaultConnection);
    }
}