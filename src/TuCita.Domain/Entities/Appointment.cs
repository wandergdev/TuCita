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
    
    /// <summary>
    /// Crea una nueva cita.
    /// Una cita siempre debe pertenecer a un profesional y contener
    /// toda la información necesaria para poder ser agendada.
    /// </summary>
    public Appointment(
        AppointmentId id,
        ProfessionalId professionalId,
        string customerName,
        PhoneNumber customerPhone,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime)
    {
        // Un Appointment nunca puede existir sin un identificador.
        ArgumentNullException.ThrowIfNull(id);

        // Toda cita debe pertenecer a un profesional.
        ArgumentNullException.ThrowIfNull(professionalId);

        // El teléfono del cliente es obligatorio.
        ArgumentNullException.ThrowIfNull(customerPhone);

        // El nombre del cliente es obligatorio.
        if (string.IsNullOrWhiteSpace(customerName))
            throw new ArgumentException(
                "El nombre del cliente es obligatorio.",
                nameof(customerName));

        // La hora de inicio debe ser menor que la hora de fin.
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

        // Toda cita inicia como programada.
        Status = AppointmentStatus.Scheduled;

        // Fecha de creación de la cita.
        CreatedAt = DateTime.UtcNow;
    }
}
