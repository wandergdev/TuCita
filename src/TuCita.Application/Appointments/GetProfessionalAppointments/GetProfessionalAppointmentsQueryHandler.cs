using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Obtiene una página de citas del profesional autenticado para su panel.
/// </summary>
public sealed class GetProfessionalAppointmentsQueryHandler
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 100;

    private readonly IAppointmentRepository _appointmentRepository;

    public GetProfessionalAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    public async Task<GetProfessionalAppointmentsResponse> HandleAsync(
        GetProfessionalAppointmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var skip = Math.Max(query.Skip, 0);
        var take = query.Take <= 0 ? DefaultPageSize : Math.Min(query.Take, MaxPageSize);
        var ascending = query.Status == AppointmentStatus.Scheduled;

        // Se pide una fila extra para saber si hay más páginas sin un COUNT(*) aparte.
        var appointments = await _appointmentRepository.GetByProfessionalAsync(
            query.ProfessionalId, query.Status, ascending, skip, take + 1, cancellationToken);

        var hasMore = appointments.Count > take;
        var page = appointments.Take(take).Select(ToSummary).ToList();

        return new GetProfessionalAppointmentsResponse(page, hasMore);
    }

    private static AppointmentSummary ToSummary(Appointment appointment) => new(
        appointment.Id,
        appointment.CustomerName,
        appointment.CustomerPhone.Value,
        appointment.Date,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status);
}
