namespace TuCita.Application.Abstractions;

/// <summary>
/// Abstrae la hora actual para que los casos de uso sensibles al tiempo
/// (como los recordatorios) sean testeables de forma determinística.
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
}
