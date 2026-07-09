using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.TestData;

public static class AppointmentTestData
{
    public static Appointment Create()
    {
        return new Appointment(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));
    }
}