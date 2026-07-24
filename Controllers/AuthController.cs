using ApiBase.API.Extensions;
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

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <param name="request">Los datos del usuario a registrar (username, email, password).</param>
    /// <returns>La confirmación de la creación del usuario.</returns>
    /// <response code="201">Retorna el usuario recién creado.</response>
    /// <response code="400">Si los datos enviados son inválidos (ej. contraseña muy débil).</response>
    /// <response code="409">Si el email o el nombre de usuario ya se encuentran registrados.</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(Common.ApiResponse<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // Los errores (409 Conflict, 400 Bad Request) ahora son lanzados como excepciones
        // por el servicio y capturados por el middleware, así que si llega aquí, todo salió bien.
        var response = await _authService.RegisterAsync(request);
        
        // Usamos el extension method que estandariza la respuesta y agrega el mensaje
        return this.ApiCreated(nameof(Register), new {}, response);
    }

    /// <summary>
    /// Inicia sesión y obtiene un JWT Access Token y un Refresh Token.
    /// </summary>
    /// <param name="request">Credenciales del usuario (email y password).</param>
    /// <returns>Los tokens de acceso y refresco si las credenciales son válidas.</returns>
    /// <response code="200">Credenciales válidas, retorna los tokens.</response>
    /// <response code="400">Datos inválidos enviados en la petición.</response>
    /// <response code="401">Credenciales incorrectas (Email o contraseña erróneos).</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(Common.ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var response = await _authService.LoginAsync(request);
        if (!response.Success) 
            throw new Common.Exceptions.UnauthorizedException(response.Message);
            
        return this.ApiOk(response, "Inicio de sesión exitoso");
    }

    /// <summary>
    /// Refresca un Access Token expirado utilizando un Refresh Token válido.
    /// </summary>
    /// <param name="request">El token actual y el refresh token.</param>
    [HttpPost("refresh-token")]
    [ProducesResponseType(typeof(Common.ApiResponse<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] TokenRequestDto request)
    {
        var response = await _authService.RefreshTokenAsync(request);
        if (!response.Success) 
            throw new Common.Exceptions.UnauthorizedException("El refresh token es inválido o expiró.");
            
        return this.ApiOk(response, "Token refrescado con éxito");
    }

    /// <summary>
    /// Cierra la sesión revocando el Refresh Token.
    /// </summary>
    /// <param name="request">El token actual y el refresh token a revocar.</param>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Common.ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Logout([FromBody] TokenRequestDto request)
    {
        var success = await _authService.LogoutAsync(request.RefreshToken);
        if (!success) 
            throw new Common.Exceptions.BadRequestException("Fallo al cerrar sesión.");
            
        return this.ApiOk<object>(new {}, "Cierre de sesión exitoso");
    }
}
