using ApiBase.Application.DTOs;
using ApiBase.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var response = await _authService.RegisterAsync(request);
        if (!response.Success) return BadRequest(response);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.Success) return Unauthorized(response);
        return Ok(response);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (!response.Success) return Unauthorized(response);
        return Ok(response);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] TokenRequestDto request)
    {
        var success = await _authService.LogoutAsync(request.RefreshToken);
        if (!success) return BadRequest(new { Message = "Failed to logout" });
        return Ok(new { Message = "Logged out successfully" });
    }
}
