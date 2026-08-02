using System.Security.Claims;
using TuCita.Api.Contracts.Appointments;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Appointments.CancelAppointment;
using TuCita.Application.Appointments.CompleteAppointment;
using TuCita.Application.Appointments.GetAppointmentStats;
using TuCita.Application.Appointments.GetProfessionalAppointments;
using TuCita.Application.Appointments.MarkAppointmentAsNoShow;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Api.Endpoints;

public static class AppointmentEndpoints
{
    public static void MapAppointmentEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/professionals/{slug}/appointments", async (
                string slug,
                BookAppointmentRequest request,
                BookAppointmentCommandHandler handler,
                CancellationToken cancellationToken) =>
            {
                var command = new BookAppointmentCommand(
                    slug,
                    request.CustomerName,
                    request.CustomerPhone,
                    request.Date,
                    request.StartTime);

                var response = await handler.HandleAsync(command, cancellationToken);

                return Results.Created($"/api/appointments/{response.AppointmentId.Value}", response);
            })
            .WithTags("Appointments");

        var group = app.MapGroup("/api/appointments")
            .WithTags("Appointments")
            .RequireAuthorization();

        group.MapGet("/mine", async (
            ClaimsPrincipal user,
            string? status,
            int? skip,
            int? take,
            GetProfessionalAppointmentsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetProfessionalAppointmentsQuery(
                    GetProfessionalId(user),
                    ParseStatus(status),
                    skip ?? 0,
                    take ?? 0),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapGet("/stats", async (
            ClaimsPrincipal user,
            string? range,
            GetAppointmentStatsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetAppointmentStatsQuery(GetProfessionalId(user), ParseRange(range)),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapPost("/{id}/complete", async (
            string id,
            ClaimsPrincipal user,
            CompleteAppointmentCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new CompleteAppointmentCommand(new AppointmentId(id), GetProfessionalId(user)),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/{id}/cancel", async (
            string id,
            ClaimsPrincipal user,
            CancelAppointmentCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new CancelAppointmentCommand(new AppointmentId(id), GetProfessionalId(user)),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/{id}/no-show", async (
            string id,
            ClaimsPrincipal user,
            MarkAppointmentAsNoShowCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new MarkAppointmentAsNoShowCommand(new AppointmentId(id), GetProfessionalId(user)),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static ProfessionalId GetProfessionalId(ClaimsPrincipal user)
        => new(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static AppointmentStatus? ParseStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return null;

        if (!Enum.TryParse<AppointmentStatus>(status, ignoreCase: true, out var parsed))
            throw new ArgumentException("El estado indicado no es válido.");

        return parsed;
    }

    private static StatsRange ParseRange(string? range)
    {
        if (string.IsNullOrWhiteSpace(range))
            return StatsRange.Day;

        if (!Enum.TryParse<StatsRange>(range, ignoreCase: true, out var parsed))
            throw new ArgumentException("El rango indicado no es válido.");

        return parsed;
    }
}
