using TuCita.Domain.Common;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Entities;

public sealed class Professional : Entity<ProfessionalId>
{
    /// <summary>
    /// Nombre completo del profesional.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Correo electrónico del profesional.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Número telefónico del profesional.
    /// </summary>
    public PhoneNumber PhoneNumber { get; private set; }

    /// <summary>
    /// Identificador público utilizado en la URL.
    /// Ejemplo:
    /// tucita.com/jose-barber
    /// </summary>
    public Slug Slug { get; private set; }

    /// <summary>
    /// Categoría del profesional.
    /// En la V1 solamente tiene fines informativos.
    /// </summary>
    public Category Category { get; private set; }

    /// <summary>
    /// Duración de las citas del profesional.
    /// </summary>
    public AppointmentDuration AppointmentDuration { get; private set; }

    /// <summary>
    /// Horario habitual del profesional.
    /// </summary>
    public WorkingSchedule WorkingSchedule { get; private set; }

    /// <summary>
    /// Indica si el profesional puede recibir nuevas citas.
    /// </summary>
    public bool IsActive { get; private set; }

    public Professional(
        ProfessionalId id,
        string fullName,
        Email email,
        PhoneNumber phoneNumber,
        Slug slug,
        Category category,
        AppointmentDuration appointmentDuration,
        WorkingSchedule workingSchedule)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre del profesional es obligatorio.", nameof(fullName));
        FullName = fullName.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Category = category;
        AppointmentDuration = appointmentDuration
            ?? throw new ArgumentNullException(nameof(appointmentDuration));
        WorkingSchedule = workingSchedule
            ?? throw new ArgumentNullException(nameof(workingSchedule));
        
        // Thodo profesional inicia activo al momento de registrarse
        
        
        IsActive = true;
    }
}