using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using Dapper;
using JWT_Implementation.Constants;
using JWT_Implementation.DTOs;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using JWT_Implementation.Exceptions;
using JWT_Implementation.Helpers;
using JWT_Implementation.Services.Interfaces;
using JWT_Implementation.TokenService;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JWT_Implementation.Services;

public class UserService : IUserService
{
    
    private readonly ConnectionStrings _options;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;

    public UserService(IOptions<ConnectionStrings> options, ITokenService tokenService, IEmailService emailService)
    {
        _options = options.Value;
        _tokenService = tokenService;
        _emailService = emailService;
    }
    
    
    
    public async Task<ReadUserDto> RegisterUserAsync(UserDto userDto)
    {
        using var connection = CreateSqlConnection();

        var numberOfUsersWithCertainEmail = await connection.QueryFirstOrDefaultAsync<int>($"SELECT COUNT(*) as count FROM Users WHERE email = '{userDto.Email}'");

        if (numberOfUsersWithCertainEmail > 0)
        {
            throw new UserAlreadyExistsException();
        }
        
        using var hmac = new HMACSHA512();

        var newUser = new User
        {
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Occupation = userDto.Occupation,
            Username = userDto.FirstName + userDto.LastName, //Mislim da i ovo treba izmijeniti
            PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(userDto.Password!)),
            PasswordSalt = hmac.Key,
            AppRole = Role.User,
            Email = userDto.Email,
            IsActivated = false,
            EmailVerificationCode = CodeGenerator.GenerateCode(),
            CodeExpirationTime = DateTime.Now.AddMinutes(10)
        };

        if (String.IsNullOrWhiteSpace(newUser.FirstName) || String.IsNullOrWhiteSpace(newUser.LastName))
        {
            throw new InvalidDataException();
        }

        var checkUserId = newUser.Id;

        var newHero = await connection.ExecuteAsync(
            "insert into Users (Firstname, Lastname, Occupation, Username, PasswordHash, PasswordSalt, AppRole, Email, IsActivated, EmailVerificationCode, CodeExpirationTime) values (@FirstName, @Lastname, @Occupation, @Username, @PasswordHash, @PasswordSalt, @AppRole, @Email,  @IsActivated, @EmailVerificationCode, @CodeExpirationTime)",
            newUser);

     // _emailService.SendLinkEmailAsync(newUser.Email!, newUser.EmailVerificationCode);
        
        return new ReadUserDto
        {
            Id = newUser.Id,
            Email = newUser.Email,
        };
    }

    public async Task<ReadUserDto> LoginUserAsync(LoginDto loginDto)
    {
        using var connection = CreateSqlConnection();

        var existingUser =
            await connection.QueryFirstOrDefaultAsync<User>($"SELECT * FROM Users WHERE Email = '{loginDto.Email}'");

        if (existingUser is null)
        {
            throw new InvalidUserNameOrPasswordException("Invalid Username or Password");
        }

        if (!existingUser.IsActivated)
        {
            throw new AccountNotActiveException();
        }

        using var hmac = new HMACSHA512(existingUser.PasswordSalt!);

        var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(loginDto.Password!));

        for (int i = 0; i < computedHash.Length; i++)
        {
            if (computedHash[i] != existingUser.PasswordHash![i])
            {
                throw new InvalidUserNameOrPasswordException("Invalid Username or Password");
            }
        }

        return new ReadUserDto
        {
            Email = existingUser.Email,
            Token = _tokenService.CreateToken(existingUser),
        };
    }

    public async Task<IEnumerable<User>> GetUsersAsync()
    {
        using var connection = CreateSqlConnection();

        var users = await connection.QueryAsync<User>("SELECT * FROM Users");

        return users;
    }

    public async Task UpdateUserRoleAsync(int id, string role)
    {
        using var connection = CreateSqlConnection();


       
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

        var updateRole = await connection.ExecuteAsync("UPDATE Users SET AppRole = @AppRole WHERE Id = @Id", new
        {
            AppRole = newRole,
            Id = id
        });
        
    }

    public async Task<IEnumerable<User>> GetAllUsersOnProjectAsync(int projectId)
    {
        using var connection = CreateSqlConnection();

        var usersOnProject = await connection.QueryAsync<User>($"SELECT Users.Id, Users.Email, Users.FirstName, Users.LastName, Users.Username, Users.Occupation, Projects.ProjectName FROM Users JOIN UsersProjectsRelation ON Users.Id = UsersProjectsRelation.UserId JOIN Projects ON UsersProjectsRelation.ProjectId = Projects.Id WHERE Projects.Id = '{projectId}'");

        return usersOnProject;

    }

    public async Task<ReadUserDto> ActivateUserAsync(AcitvateUserDto acitvateUserDto)
    {
        using var connection = CreateSqlConnection();

        var existingUser = await connection.QueryFirstOrDefaultAsync<User>($"SELECT * FROM Users WHERE Email = '{acitvateUserDto.Email}'");

        if (DateTime.Now > existingUser.CodeExpirationTime)
        {
            throw new CodeExpiredException();
        }

        if (!existingUser.EmailVerificationCode!.Equals(acitvateUserDto.VerificationCode))
        {
            throw new InvalidVerificationCodeException();
        }

        await connection.ExecuteAsync($"UPDATE Users SET IsActivated = '{true}', EmailVerificationCode = '{null}', CodeExpirationTime = '{null}' WHERE Id ='{existingUser.Id}'");
        
        return new ReadUserDto
        {
            Email = existingUser.Email,
            Token = _tokenService.CreateToken(existingUser),
        };
    }

    private SqlConnection CreateSqlConnection()
    {
        return new SqlConnection(_options.DefaultConnection);
    }
}