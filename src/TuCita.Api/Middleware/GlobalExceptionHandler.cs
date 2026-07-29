using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TuCita.Application.Common.Exceptions;

namespace TuCita.Api.Middleware;

/// <summary>
/// Traduce las excepciones de Application/Domain a respuestas HTTP
/// consistentes, evitando repetir try/catch en cada endpoint.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            ForbiddenException => (StatusCodes.Status403Forbidden, "Acceso no permitido"),
            UnauthorizedException => (StatusCodes.Status401Unauthorized, "No autorizado"),
            InvalidOperationException => (StatusCodes.Status409Conflict, "Conflicto"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno")
        };

        httpContext.Response.StatusCode = statusCode;

        await httpContext.Response.WriteAsJsonAsync(
            new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Detail = statusCode == StatusCodes.Status500InternalServerError
                    ? "Ocurrió un error inesperado."
                    : exception.Message
            },
            cancellationToken);

        return true;
    }
}
