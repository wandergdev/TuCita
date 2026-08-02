using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Entities;

public class Appointment
{
    public AppointmentId Id { get; private set; }

    public ProfessionalId ProfessionalId { get; private set; }

    public string CustomerName { get; private set; }

    public PhoneNumber CustomerPhone { get; private set; }

    public DateOnly Date { get; private set; }

    public TimeOnly StartTime { get; private set; }

    public TimeOnly EndTime { get; private set; }

    public AppointmentStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? ReminderSentAt { get; private set; }

    /// <summary>
    /// Crea una nueva cita.
    /// Una cita siempre debe pertenecer a un profesional y contener
    /// toda la información necesaria para poder ser agendada.
    /// </summary>
    private Appointment(
        AppointmentId id,
        ProfessionalId professionalId,
        string customerName,
        PhoneNumber customerPhone,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        ArgumentNullException.ThrowIfNull(id);
        ArgumentNullException.ThrowIfNull(professionalId);
        ArgumentNullException.ThrowIfNull(customerPhone);

        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException(
                "El nombre del cliente es obligatorio.",
                nameof(customerName));

        if (startTime >= endTime)
            throw new ArgumentException(
                "La hora de inicio debe ser menor que la hora de fin.");

        Id = id;
        ProfessionalId = professionalId;
        CustomerName = customerName.Trim();
        CustomerPhone = customerPhone;
        Date = date;
        StartTime = startTime;
        EndTime = endTime;

        Status = AppointmentStatus.Scheduled;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Cancela una cita previamente programada.
    /// </summary>
    public void Cancel()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo una cita programada puede cancelarse.");

        Status = AppointmentStatus.Cancelled;
    }

    /// <summary>
    /// Marca una cita como completada.
    /// </summary>
    public void Complete()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo una cita programada puede completarse.");

        Status = AppointmentStatus.Completed;
    }

    /// <summary>
    /// Marca una cita como ausencia del cliente.
    /// </summary>
    public void MarkAsNoShow()
    {
        if (Status != AppointmentStatus.Scheduled)
            throw new InvalidOperationException(
                "Solo una cita programada puede marcarse como No Show.");

        Status = AppointmentStatus.NoShow;
    }

    /// <summary>
    /// Registra que ya se envió el recordatorio de esta cita,
    /// para no volver a enviarlo.
    /// </summary>
    public void MarkReminderSent(DateTime sentAt)
    {
        ReminderSentAt = sentAt;
    }

    public static Appointment Create(
        AppointmentId id,
        ProfessionalId professionalId,
        string customerName,
        PhoneNumber customerPhone,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        return new Appointment(
            id,
            professionalId,
            customerName,
            customerPhone,
            date,
            startTime,
            endTime);
    }
}