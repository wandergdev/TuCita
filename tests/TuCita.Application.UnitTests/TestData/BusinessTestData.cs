using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.UnitTests.TestData;

public static class BusinessTestData
{
    public static Business Create(
        string id = "business-001",
        string email = "peluqueria@email.com",
        string slug = "peluqueria-lisbeth",
        string passwordHash = "hashed-password",
        bool emailVerified = true)
    {
        var business = Business.Create(
            new BusinessId(id),
            "Peluquería Lisbeth",
            Email.Create(email),
            Slug.Create(slug),
            passwordHash);

        if (emailVerified)
            business.VerifyEmail();

        return business;
    }
}
