using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.UnitTests.TestData;

public static class ProfessionalTestData
{
    public static Professional Create(
        string id = "professional-001",
        string email = "wander@email.com",
        string slug = "wander-genao",
        string passwordHash = "hashed-password") => Professional.Create(
            new ProfessionalId(id),
            "Wander Genao",
            Email.Create(email),
            PhoneNumber.Create("8095551234"),
            Slug.Create(slug),
            Category.Barber,
            AppointmentDuration.Create(30),
            WorkingScheduleTestData.Create(),
            passwordHash);
}
