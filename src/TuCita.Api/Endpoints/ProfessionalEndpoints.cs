using System.Security.Claims;
using TuCita.Api.Contracts.Professionals;
using TuCita.Application.Appointments.CheckExistingAppointment;
using TuCita.Application.Professionals.ChangePassword;
using TuCita.Application.Professionals.ForgotPassword;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.Professionals.GetMyWorkingSchedule;
using TuCita.Application.Professionals.GetProfessionalPublicProfile;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.Professionals.Portfolio.AddPortfolioPhoto;
using TuCita.Application.Professionals.Portfolio.CreatePortfolioUploadSignature;
using TuCita.Application.Professionals.Portfolio.DeletePortfolioPhoto;
using TuCita.Application.Professionals.Portfolio.GetMyPortfolio;
using TuCita.Application.Professionals.RegisterProfessional;
using TuCita.Application.Professionals.ResendVerificationEmail;
using TuCita.Application.Professionals.ResetPassword;
using TuCita.Application.Professionals.ScheduleExceptions.CreateScheduleException;
using TuCita.Application.Professionals.ScheduleExceptions.DeleteScheduleException;
using TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;
using TuCita.Application.Professionals.UpdateWorkingSchedule;
using TuCita.Application.Professionals.VerifyEmail;
using TuCita.Domain.Identifiers;
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
            var workingSchedule = ToWorkingSchedule(request.WorkingSchedule);

            var command = new RegisterProfessionalCommand(
                request.FullName,
                request.Email,
                request.PhoneNumber,
                request.Slug,
                request.Category,
                request.AppointmentDurationMinutes,
                workingSchedule,
                request.Password,
                request.InviteToken);

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

        group.MapPost("/verify-email", async (
            VerifyEmailRequest request,
            VerifyEmailCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new VerifyEmailCommand(request.Token), cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/resend-verification", async (
            ResendVerificationEmailRequest request,
            ResendVerificationEmailCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new ResendVerificationEmailCommand(request.Email), cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            ForgotPasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new ForgotPasswordCommand(request.Email), cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            ResetPasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ResetPasswordCommand(request.Token, request.NewPassword),
                cancellationToken);

            return Results.NoContent();
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

        group.MapGet("/{slug}/appointments/existing", async (
            string slug,
            string customerPhone,
            DateOnly date,
            CheckExistingAppointmentQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CheckExistingAppointmentQuery(slug, customerPhone, date),
                cancellationToken);

            return Results.Ok(response);
        });

        var meGroup = app.MapGroup("/api/professionals/me")
            .WithTags("Professionals")
            .RequireAuthorization("ProfessionalOnly");

        meGroup.MapGet("/schedule", async (
            ClaimsPrincipal user,
            GetMyWorkingScheduleQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetMyWorkingScheduleQuery(GetProfessionalId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapPut("/schedule", async (
            UpdateWorkingScheduleRequest request,
            ClaimsPrincipal user,
            UpdateWorkingScheduleCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var workingSchedule = ToWorkingSchedule(request.WorkingSchedule);

            await handler.HandleAsync(
                new UpdateWorkingScheduleCommand(GetProfessionalId(user), workingSchedule),
                cancellationToken);

            return Results.NoContent();
        });

        meGroup.MapGet("/schedule/exceptions", async (
            ClaimsPrincipal user,
            GetScheduleExceptionsQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetScheduleExceptionsQuery(GetProfessionalId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapPost("/schedule/exceptions", async (
            CreateScheduleExceptionRequest request,
            ClaimsPrincipal user,
            CreateScheduleExceptionCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateScheduleExceptionCommand(
                GetProfessionalId(user),
                request.StartDate,
                request.EndDate,
                request.StartTime,
                request.EndTime,
                request.LunchBreakStartTime,
                request.LunchBreakEndTime,
                request.Message);

            var response = await handler.HandleAsync(command, cancellationToken);

            return Results.Created(
                $"/api/professionals/me/schedule/exceptions/{response.ScheduleExceptionId.Value}",
                response);
        });

        meGroup.MapDelete("/schedule/exceptions/{id}", async (
            string id,
            ClaimsPrincipal user,
            DeleteScheduleExceptionCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new DeleteScheduleExceptionCommand(new ScheduleExceptionId(id), GetProfessionalId(user)),
                cancellationToken);

            return Results.NoContent();
        });

        meGroup.MapPost("/portfolio/upload-signature", async (
            ClaimsPrincipal user,
            CreatePortfolioUploadSignatureQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CreatePortfolioUploadSignatureQuery(GetProfessionalId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapPost("/portfolio", async (
            AddPortfolioPhotoRequest request,
            ClaimsPrincipal user,
            AddPortfolioPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddPortfolioPhotoCommand(
                GetProfessionalId(user),
                request.ImageUrl,
                request.PublicId,
                request.Caption);

            var response = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/api/professionals/me/portfolio/{response.PortfolioPhotoId.Value}", response);
        });

        meGroup.MapGet("/portfolio", async (
            ClaimsPrincipal user,
            GetMyPortfolioQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetMyPortfolioQuery(GetProfessionalId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapDelete("/portfolio/{id}", async (
            string id,
            ClaimsPrincipal user,
            DeletePortfolioPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new DeletePortfolioPhotoCommand(new PortfolioPhotoId(id), GetProfessionalId(user)),
                cancellationToken);

            return Results.NoContent();
        });

        meGroup.MapPost("/change-password", async (
            ChangePasswordRequest request,
            ClaimsPrincipal user,
            ChangePasswordCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangePasswordCommand(
                GetProfessionalId(user),
                request.CurrentPassword,
                request.NewPassword);

            await handler.HandleAsync(command, cancellationToken);

            return Results.NoContent();
        });
    }

    private static ProfessionalId GetProfessionalId(ClaimsPrincipal user)
        => new(user.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static WorkingSchedule ToWorkingSchedule(IEnumerable<WorkingScheduleRuleRequest> rules)
        => new(rules.Select(rule =>
        {
            if (rule.LunchBreakStartTime.HasValue != rule.LunchBreakEndTime.HasValue)
                throw new ArgumentException(
                    "Debe indicar hora de inicio y de fin de almuerzo, o ninguna de las dos.");

            var lunchBreak = rule.LunchBreakStartTime.HasValue && rule.LunchBreakEndTime.HasValue
                ? TimeRange.Create(rule.LunchBreakStartTime.Value, rule.LunchBreakEndTime.Value)
                : null;

            return new ScheduleRule(
                new WorkDays(rule.Days),
                TimeRange.Create(rule.StartTime, rule.EndTime),
                lunchBreak);
        }));
}
