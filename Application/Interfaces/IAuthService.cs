using ApiBase.Application.DTOs;

namespace ApiBase.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto request);
    Task<bool> LogoutAsync(string refreshToken);
}
