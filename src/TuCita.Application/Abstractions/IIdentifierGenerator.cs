using TuCita.Domain.Identifiers;

namespace TuCita.Application.Abstractions;

/// <summary>
/// Genera identificadores únicos para las entidades del dominio.
/// </summary>
public interface IIdentifierGenerator
{
    ProfessionalId NewProfessionalId();

    AppointmentId NewAppointmentId();

    ScheduleExceptionId NewScheduleExceptionId();

    BusinessId NewBusinessId();

    BusinessInviteId NewBusinessInviteId();

    PortfolioPhotoId NewPortfolioPhotoId();

    BusinessPhotoId NewBusinessPhotoId();
}