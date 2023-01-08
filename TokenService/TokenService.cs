using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using JWT_Implementation.Entities;
using JWT_Implementation.EnvironmentSettings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace JWT_Implementation.TokenService;

public class TokenService : ITokenService
{
    private readonly SymmetricSecurityKey _key;
    
    public TokenService(IOptions<SecretConfiguration> options)
    {
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(options.Value.SecretKey!)); //options.Value;
    }
    
    public string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Role, user.AppRole.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), //Trenutno storanje Id-a u Tokenu
            new Claim(JwtRegisteredClaimNames.NameId, user.Username!),

            //new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()) ==> Da li vako storati Id u Tokenu

        };

        var credentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.Now.AddDays(7),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);

        //var tokenhandler = new JwtSecurityTokenHandler();

        //var token = tokenhandler.CreateToken(tokenDescriptor); //kreiranje tokena

        //return tokenhandler.WriteToken(token); //readanje tokena
    }
}