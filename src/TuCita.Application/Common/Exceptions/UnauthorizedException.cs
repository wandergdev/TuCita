namespace TuCita.Application.Common.Exceptions;

/// <summary>
/// Se lanza cuando las credenciales proporcionadas son inválidas.
/// </summary>
public sealed class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message)
    {
    }
}
