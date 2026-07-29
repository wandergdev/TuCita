namespace TuCita.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando el profesional autenticado intenta operar
/// sobre un recurso que no le pertenece.
/// </summary>
public sealed class ForbiddenException : Exception
{
    public ForbiddenException(string message) : base(message)
    {
    }
}
