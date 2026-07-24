namespace ApiBase.Common;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public T? Data { get; init; }
    public object? Errors { get; init; }
    public PaginationMeta? Pagination { get; init; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    // ── Factories ──────────────────────────────────────────────────────────────

    /// <summary>Respuesta exitosa con datos</summary>
    public static ApiResponse<T> SuccessResult(T? data, string message = "Operación exitosa")
        => new()
        {
            Success = true,
            Message = message,
            Data = data
        };

    /// <summary>Respuesta exitosa con paginación</summary>
    public static ApiResponse<T> SuccessPaged(
        T? data,
        PaginationMeta pagination,
        string message = "Operación exitosa")
        => new()
        {
            Success = true,
            Message = message,
            Data = data,
            Pagination = pagination
        };

    /// <summary>Respuesta de error</summary>
    public static ApiResponse<T> Failure(string message, object? errors = null)
        => new()
        {
            Success = false,
            Message = message,
            Errors = errors
        };

    /// <summary>Respuesta de validación fallida</summary>
    public static ApiResponse<T> ValidationError(object errors)
        => new()
        {
            Success = false,
            Message = "Error de validación",
            Errors = errors
        };
}
