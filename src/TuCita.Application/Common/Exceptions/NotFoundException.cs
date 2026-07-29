namespace TuCita.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando el recurso solicitado no existe.
/// </summary>
public sealed class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }
}
