using TuCita.Application.Abstractions;

namespace TuCita.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
}
