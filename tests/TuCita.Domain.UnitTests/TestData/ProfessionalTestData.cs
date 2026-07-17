using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.TestData;

/// <summary>
/// Proporciona objetos válidos para las pruebas unitarias.
/// Evita duplicar código y mantiene el foco de cada prueba
/// en la regla de negocio que desea validar.
/// </summary>
public static class ProfessionalTestData
{
    public static Professional Create()
    {
        return  Professional.Create(
            new ProfessionalId("professional-001"),
            "Wander Genao",
            Email.Create("wander@email.com"),
            PhoneNumber.Create("8095551234"),
            Slug.Create("wander-genao"),
            Category.Barber,
            AppointmentDuration.Create(30),
            CreateWorkingSchedule());
    }

    public static WorkingSchedule CreateWorkingSchedule()
    {
        return new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays(
                [
                    DayOfWeek.Monday,
                    DayOfWeek.Tuesday,
                    DayOfWeek.Wednesday,
                    DayOfWeek.Thursday,
                    DayOfWeek.Friday
                ]),
                TimeRange.Create(
                    new TimeOnly(8, 0),
                    new TimeOnly(18, 0)))
        ]);
    }
}