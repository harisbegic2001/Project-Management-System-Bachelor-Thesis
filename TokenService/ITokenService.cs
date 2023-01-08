using JWT_Implementation.Entities;

namespace JWT_Implementation.TokenService;

public interface ITokenService
{
    string CreateToken(User user);
}