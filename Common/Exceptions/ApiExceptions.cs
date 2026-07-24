using Microsoft.AspNetCore.Http;

namespace ApiBase.Common.Exceptions;

/// <summary>Excepción base para errores de negocio con código HTTP</summary>
public abstract class ApiException : Exception
{
    public int StatusCode { get; }
    public object? Errors { get; }

    protected ApiException(string message, int statusCode, object? errors = null) : base(message)
    {
        StatusCode = statusCode;
        Errors = errors;
    }
}

/// <summary>404 — Recurso no encontrado</summary>
public class NotFoundException : ApiException
{
    public NotFoundException(string message)
        : base(message, StatusCodes.Status404NotFound) { }
}

/// <summary>400 — Solicitud inválida de negocio</summary>
public class BadRequestException : ApiException
{
    public BadRequestException(string message)
        : base(message, StatusCodes.Status400BadRequest) { }
}

/// <summary>409 — Conflicto (duplicado)</summary>
public class ConflictException : ApiException
{
    public ConflictException(string message)
        : base(message, StatusCodes.Status409Conflict) { }

    public ConflictException(string property, string message)
        : base("Error de conflicto", StatusCodes.Status409Conflict, new Dictionary<string, string[]> { { property, new[] { message } } }) { }
}

/// <summary>403 — Sin permisos</summary>
public class ForbiddenException : ApiException
{
    public ForbiddenException(string message = "No tienes permisos para realizar esta acción")
        : base(message, StatusCodes.Status403Forbidden) { }
}

/// <summary>422 — Error de lógica de negocio</summary>
public class UnprocessableException : ApiException
{
    public UnprocessableException(string message)
        : base(message, StatusCodes.Status422UnprocessableEntity) { }
}

/// <summary>401 — No autenticado</summary>
public class UnauthorizedException : ApiException
{
    public UnauthorizedException(string message = "No autenticado")
        : base(message, StatusCodes.Status401Unauthorized) { }
}
