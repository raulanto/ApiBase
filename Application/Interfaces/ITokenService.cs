using System.Security.Claims;
using ApiBase.Domain.Entities;

namespace ApiBase.Application.Interfaces;

public interface ITokenService
{
    string GenerateAccessToken(User user, IEnumerable<Permission> permissions);
    string GenerateRefreshToken();
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
