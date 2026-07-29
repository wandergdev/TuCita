using TuCita.Api.Contracts.Professionals;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.Professionals.GetProfessionalPublicProfile;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.Professionals.RegisterProfessional;
using TuCita.Domain.ValueObjects;

namespace TuCita.Api.Endpoints;

public static class ProfessionalEndpoints
{
    public static void MapProfessionalEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/professionals").WithTags("Professionals");

        group.MapPost("/register", async (
            RegisterProfessionalRequest request,
            RegisterProfessionalCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var workingSchedule = new WorkingSchedule(request.WorkingSchedule.Select(rule =>
                new ScheduleRule(
                    new WorkDays(rule.Days),
                    TimeRange.Create(rule.StartTime, rule.EndTime))));

            var command = new RegisterProfessionalCommand(
                request.FullName,
                request.Email,
                request.PhoneNumber,
                request.Slug,
                request.Category,
                request.AppointmentDurationMinutes,
                workingSchedule,
                request.Password);

            var response = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/api/professionals/{request.Slug}", response);
        });

        group.MapPost("/login", async (
            LoginProfessionalRequest request,
            LoginProfessionalCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new LoginProfessionalCommand(request.Email, request.Password),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapGet("/{slug}", async (
            string slug,
            GetProfessionalPublicProfileQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetProfessionalPublicProfileQuery(slug),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapGet("/{slug}/availability", async (
            string slug,
            DateOnly date,
            GetAvailabilityQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetAvailabilityQuery(slug, date),
                cancellationToken);

            return Results.Ok(response);
        });
    }
}
