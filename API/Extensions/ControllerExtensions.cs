using ApiBase.Common;
using Microsoft.AspNetCore.Mvc;

namespace ApiBase.API.Extensions;

public static class ControllerExtensions
{
    public static IActionResult ApiOk<T>(this ControllerBase controller, T data, string? message = null)
        => controller.Ok(ApiResponse<T>.SuccessResult(data, message ?? "Operación exitosa"));

    public static IActionResult ApiCreated<T>(
        this ControllerBase controller,
        string actionName,
        object routeValues,
        T data)
        => controller.CreatedAtAction(
            actionName,
            routeValues,
            ApiResponse<T>.SuccessResult(data, "Recurso creado exitosamente")
        );

    public static IActionResult ApiNoContent(this ControllerBase controller)
        => controller.Ok(ApiResponse<object>.SuccessResult(null, "Operación completada"));
}
