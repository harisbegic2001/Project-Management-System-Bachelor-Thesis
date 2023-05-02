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
    private readonly ConnectionStrings _options;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public UsersController(IOptions<ConnectionStrings> options, ITokenService tokenService, IEmailService emailService)
    {
        _options = options.Value;
        _tokenService = tokenService;
        _emailService = emailService;
    }

    /// <summary>
    /// Register function
    /// </summary>
    /// <returns> Json Web Token and created User</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ReadUserDto>> RegisterUserAsync(UserDto userDto)
    {
        using var connection = CreateSqlConnection();

        var numberOfUsersWithCertainEmail = await connection.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) as count FROM Users WHERE email = '{userDto.Email}'");

        if (numberOfUsersWithCertainEmail > 0)
        {
            return BadRequest("User with this email already exists!!");
        }
        
        using var hmac = new HMACSHA512();

        var newUser = new User
        {
            Id = 0, //Izbaciti 
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Occupation = userDto.Occupation,
            Username = userDto.FirstName + userDto.LastName, //Mislim da i ovo treba izmijeniti
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password!)),
            PasswordSalt = hmac.Key,
            AppRole = Role.User,
            Email = userDto.Email,

        };

        if (String.IsNullOrWhiteSpace(newUser.FirstName) || String.IsNullOrWhiteSpace(newUser.LastName))
        {
            return BadRequest("One or more fields are empty!");
        }

        var checkUserId = newUser.Id;

        var newHero = await connection.ExecuteAsync(
            "insert into Users (Firstname, Lastname, Occupation, Username, PasswordHash, PasswordSalt, AppRole, Email) values (@FirstName, @Lastname, @Occupation, @Username, @PasswordHash, @PasswordSalt, @AppRole, @Email)",
            newUser);

        _emailService.SendLinkEmailAsync(newUser.Email!);
        
        return Ok(new ReadUserDto
        {
            Email = newUser.Email,
            Token = _tokenService.CreateToken(newUser) //Neće trebati 
        });
    }


    [HttpPost("login")]
    public async Task<ActionResult<ReadUserDto>> LoginUserAsync(LoginDto loginDto)
    {
        using var connection = CreateSqlConnection();

        var existingUser =
            await connection.QueryFirstOrDefaultAsync<User>($"SELECT * FROM Users WHERE Email = '{loginDto.Email}'");

        if (existingUser is null)
        {
            return Unauthorized("Invalid Username or Password");
        }

        using var hmac = new HMACSHA512(existingUser.PasswordSalt!);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password!));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != existingUser.PasswordHash![i])
            {
                return Unauthorized("Invalid Username or Password");
            }
        }

        return Ok(new ReadUserDto
        {
            Email = existingUser.Email,
            Token = _tokenService.CreateToken(existingUser),
        });
    }


    [Authorize(Roles = nameof(Role.Admin))]
    [HttpGet]
    public async Task<ActionResult<List<User>>> GetUsersAsync()
    {
        using var connection = CreateSqlConnection();

        var users = await connection.QueryAsync("SELECT * FROM Users");

        return Ok(users);
    }

    [HttpPut]
    [Authorize(Roles = nameof(Role.Admin))]
    public async Task<ActionResult<User>> UpdateUserRoleAsync(int id, string role)
    {
        using var connection = CreateSqlConnection();


       var role2 = HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value;
       
        var updatedUser = await connection.QueryFirstOrDefaultAsync("SELECT * FROM Users WHERE Id = @Id", new
        {
            Id = id
        });

        if (updatedUser is null)
        {
            throw new UserNotFoundException("User not found");
        }

        if (!RoleConstants.AvailableRoles.Contains(role.ToLower()))
        {
            throw new RoleNotFoundException("Role does not exist");
        }

        var newRole = role.ToLower() == nameof(Role.Admin).ToLower() ? 0 : 1;

        var updateRole = await connection.QueryAsync("UPDATE Users SET AppRole = @AppRole WHERE Id = @Id", new
        {
            AppRole = newRole,
            Id = id
        });

        return Ok(role2);
    }


    [HttpGet("GetUsersOnProject")]
    public async Task<ActionResult<IEnumerable<User>>> GetAllUsersOnProjectAsync(int projectId)
    {
        using var connection = CreateSqlConnection();

        var usersOnProject = await connection.QueryAsync($"SELECT Users.Id, Users.FirstName, Users.LastName, Users.Username, Users.Occupation, Projects.ProjectName FROM Users JOIN UsersProjectsRelation ON Users.Id = UsersProjectsRelation.UserId JOIN Projects ON UsersProjectsRelation.ProjectId = Projects.Id WHERE Projects.Id = '{projectId}'");

        return Ok(usersOnProject);
    }


    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}