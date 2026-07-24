using System.Text.Json;
using ApiBase.Common;
using ApiBase.Common.Exceptions;
using Npgsql;
// Si se usa la excepción propia, asegúrate de no confundirla con la de FluentValidation
// usando alias o namespaces si fuera necesario, aquí usamos la nuestra o la de FluentValidation
using FV = FluentValidation;

namespace ApiBase.Infrastructure.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public ErrorHandlingMiddleware(
        RequestDelegate next,
        ILogger<ErrorHandlingMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, message, errors) = exception switch
        {
            // Excepciones propias de negocio
            ApiException apiEx => (
                apiEx.StatusCode,
                apiEx.Message,
                apiEx.Errors
            ),

            // Validación de FluentValidation lanzada manualmente
            FV.ValidationException valEx => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                (object?)valEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "General" : char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1), g => g.Select(e => e.ErrorMessage).ToArray())
            ),
            
            // Validación propia
            Common.ValidationException myValEx => (
                StatusCodes.Status400BadRequest,
                "Error de validación",
                (object?)myValEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => string.IsNullOrEmpty(g.Key) ? "General" : char.ToLowerInvariant(g.Key[0]) + g.Key.Substring(1), g => g.Select(e => e.ErrorMessage).ToArray())
            ),

            // Error de BD PostgreSQL: constraint violation (unicidad)
            PostgresException pgEx when pgEx.SqlState == "23505" => (
                StatusCodes.Status409Conflict,
                "Ya existe un registro con esos datos",
                (object?)null
            ),

            // Error de BD PostgreSQL: FK violation
            PostgresException pgEx when pgEx.SqlState == "23503" => (
                StatusCodes.Status400BadRequest,
                "El registro referencia a un dato que no existe",
                (object?)null
            ),

            // Timeout de BD / Connection issue
            PostgresException pgEx when pgEx.SqlState == "08006" || pgEx.SqlState == "57P01" => (
                StatusCodes.Status503ServiceUnavailable,
                "El servicio no está disponible, intente más tarde",
                (object?)null
            ),

            // Operación cancelada (cliente se desconectó)
            OperationCanceledException => (
                499,  // Client Closed Request (no es estándar HTTP pero muy útil)
                "La operación fue cancelada",
                (object?)null
            ),

            // Cualquier otra excepción
            _ => (
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error interno. Por favor intente más tarde.",
                (object?)null
            )
        };

        // Log apropiado según la severidad
        LogException(exception, statusCode);

        // Construir la respuesta
        var response = ApiResponse<object>.Failure(message, errors);

        // En desarrollo, agregar el stack trace
        if (_environment.IsDevelopment() && statusCode >= 500)
        {
            _logger.LogDebug("Stack trace: {StackTrace}", exception.ToString());
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, jsonOptions));
    }

    private void LogException(Exception exception, int statusCode)
    {
        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Error interno: {ExceptionType} - {Message}",
                exception.GetType().Name,
                exception.Message);
        }
        else if (statusCode >= 400)
        {
            _logger.LogWarning(
                "Error del cliente ({StatusCode}): {ExceptionType} - {Message}",
                statusCode,
                exception.GetType().Name,
                exception.Message);
        }
    }
}
