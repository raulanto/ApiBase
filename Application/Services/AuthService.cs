using ApiBase.Application.DTOs;
using ApiBase.Application.Interfaces;
using ApiBase.Domain.Entities;
using ApiBase.Domain.Interfaces;
using BCrypt.Net;

namespace ApiBase.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ITokenRepository tokenRepository,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _tokenRepository = tokenRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request)
    {
        if (await _userRepository.GetByEmailAsync(request.Email) != null)
            return new AuthResponseDto { Message = "Email already exists", Success = false };

        if (await _userRepository.GetByUsernameAsync(request.Username) != null)
            return new AuthResponseDto { Message = "Username already exists", Success = false };

        var userRole = await _roleRepository.GetByNameAsync("User");
        if (userRole == null)
            return new AuthResponseDto { Message = "Default role not found", Success = false };

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = userRole.Id,
            Name = request.Username
        };

        await _userRepository.AddAsync(user);

        return new AuthResponseDto { Message = "User registered successfully", Success = true };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.EmailOrUsername) 
                ?? await _userRepository.GetByUsernameAsync(request.EmailOrUsername);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return new AuthResponseDto { Message = "Invalid credentials", Success = false };

        var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(user.RoleId);
        var accessToken = _tokenService.GenerateAccessToken(user, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _tokenRepository.AddAsync(tokenEntity);

        return new AuthResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Success = true,
            Message = "Login successful"
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(TokenRequestDto request)
    {
        var storedToken = await _tokenRepository.GetByTokenAsync(request.RefreshToken);

        if (storedToken == null || storedToken.IsRevoked || storedToken.ExpiryDate <= DateTime.UtcNow)
            return new AuthResponseDto { Message = "Invalid or expired refresh token", Success = false };

        var user = await _userRepository.GetByIdAsync(storedToken.UserId);
        if (user == null)
            return new AuthResponseDto { Message = "User not found", Success = false };

        var permissions = await _roleRepository.GetPermissionsByRoleIdAsync(user.RoleId);
        var newAccessToken = _tokenService.GenerateAccessToken(user, permissions);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        await _tokenRepository.RevokeAsync(request.RefreshToken);

        var tokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        await _tokenRepository.AddAsync(tokenEntity);

        return new AuthResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            Success = true,
            Message = "Token refreshed"
        };
    }

    public async Task<bool> LogoutAsync(string refreshToken)
    {
        await _tokenRepository.RevokeAsync(refreshToken);
        return true;
    }
}
