using TuCita.Application.Abstractions;
using TuCita.Domain.Identifiers;
using NUlid;

namespace TuCita.Infrastructure.Services;

public sealed class UlidGenerator : IIdentifierGenerator
{
    public ProfessionalId NewProfessionalId()
        => new(Ulid.NewUlid().ToString());

    public AppointmentId NewAppointmentId()
        => new(Ulid.NewUlid().ToString());

    public ScheduleExceptionId NewScheduleExceptionId()
        => new(Ulid.NewUlid().ToString());

    public BusinessId NewBusinessId()
        => new(Ulid.NewUlid().ToString());

    public BusinessInviteId NewBusinessInviteId()
        => new(Ulid.NewUlid().ToString());

    public PortfolioPhotoId NewPortfolioPhotoId()
        => new(Ulid.NewUlid().ToString());

    public BusinessPhotoId NewBusinessPhotoId()
        => new(Ulid.NewUlid().ToString());
}