using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using TuCita.Domain.Services;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Appointments.BookAppointment;

/// <summary>
/// Se encarga de reservar una nueva cita para un profesional.
/// </summary>
public sealed class BookAppointmentCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IIdentifierGenerator _identifierGenerator;

    public BookAppointmentCommandHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository,
        IIdentifierGenerator identifierGenerator)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
    }

    public async Task<BookAppointmentResponse> HandleAsync(
        BookAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetBySlugAsync(command.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        var endTime = command.StartTime.Add(
            TimeSpan.FromMinutes(professional.AppointmentDuration.Minutes));

        var workingHours = professional.WorkingSchedule.GetWorkingHours(command.Date.DayOfWeek);
        if (workingHours is null
            || command.StartTime < workingHours.StartTime
            || endTime > workingHours.EndTime)
            throw new ArgumentException("El horario seleccionado está fuera de la jornada laboral.");

        var existingAppointments = await _appointmentRepository.GetByProfessionalAndDateAsync(
            professional.Id,
            command.Date,
            cancellationToken);

        if (!AvailabilitySlotCalculator.IsSlotAvailable(command.StartTime, endTime, existingAppointments))
            throw new InvalidOperationException("El horario seleccionado ya no está disponible.");

        var appointment = Appointment.Create(
            _identifierGenerator.NewAppointmentId(),
            professional.Id,
            command.CustomerName,
            PhoneNumber.Create(command.CustomerPhone),
            command.Date,
            command.StartTime,
            endTime);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        return new BookAppointmentResponse(appointment.Id);
    }
}
