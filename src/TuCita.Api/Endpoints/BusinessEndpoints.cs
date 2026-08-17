using System.Security.Claims;
using TuCita.Api.Contracts.Businesses;
using TuCita.Application.Businesses.CreateBusinessInvite;
using TuCita.Application.Businesses.ForgotPassword;
using TuCita.Application.Businesses.GetBusinessInvite;
using TuCita.Application.Businesses.GetBusinessPublicProfile;
using TuCita.Application.Businesses.GetMyRoster;
using TuCita.Application.Businesses.LoginBusiness;
using TuCita.Application.Businesses.Photos.AddBusinessPhoto;
using TuCita.Application.Businesses.Photos.CreateBusinessPhotoUploadSignature;
using TuCita.Application.Businesses.Photos.DeleteBusinessPhoto;
using TuCita.Application.Businesses.Photos.GetMyBusinessPhotos;
using TuCita.Application.Businesses.RegisterBusiness;
using TuCita.Application.Businesses.RemoveProfessional;
using TuCita.Application.Businesses.ResendBusinessVerificationEmail;
using TuCita.Application.Businesses.ResetPassword;
using TuCita.Application.Businesses.VerifyBusinessEmail;
using TuCita.Domain.Identifiers;

namespace TuCita.Api.Endpoints;

public static class BusinessEndpoints
{
    public static void MapBusinessEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/businesses").WithTags("Businesses");

        group.MapPost("/register", async (
            RegisterBusinessRequest request,
            RegisterBusinessCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterBusinessCommand(
                request.Name,
                request.Email,
                request.Slug,
                request.Password);

            var response = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/api/businesses/{request.Slug}", response);
        });

        group.MapPost("/login", async (
            LoginBusinessRequest request,
            LoginBusinessCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new LoginBusinessCommand(request.Email, request.Password),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapPost("/verify-email", async (
            VerifyBusinessEmailRequest request,
            VerifyBusinessEmailCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(new VerifyBusinessEmailCommand(request.Token), cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/resend-verification", async (
            ResendBusinessVerificationEmailRequest request,
            ResendBusinessVerificationEmailCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ResendBusinessVerificationEmailCommand(request.Email),
                cancellationToken);

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

        group.MapGet("/invites/{token}", async (
            string token,
            GetBusinessInviteQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(new GetBusinessInviteQuery(token), cancellationToken);

            return Results.Ok(response);
        });

        group.MapGet("/{slug}", async (
            string slug,
            GetBusinessPublicProfileQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetBusinessPublicProfileQuery(slug),
                cancellationToken);

            return Results.Ok(response);
        });

        var meGroup = app.MapGroup("/api/businesses/me")
            .WithTags("Businesses")
            .RequireAuthorization("BusinessOnly");

        meGroup.MapPost("/invites", async (
            ClaimsPrincipal user,
            CreateBusinessInviteCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CreateBusinessInviteCommand(GetBusinessId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapGet("/roster", async (
            ClaimsPrincipal user,
            GetMyRosterQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetMyRosterQuery(GetBusinessId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapDelete("/roster/{professionalId}", async (
            string professionalId,
            ClaimsPrincipal user,
            RemoveProfessionalCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new RemoveProfessionalCommand(GetBusinessId(user), new ProfessionalId(professionalId)),
                cancellationToken);

            return Results.NoContent();
        });

        meGroup.MapPost("/photos/upload-signature", async (
            ClaimsPrincipal user,
            CreateBusinessPhotoUploadSignatureQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CreateBusinessPhotoUploadSignatureQuery(GetBusinessId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapPost("/photos", async (
            AddBusinessPhotoRequest request,
            ClaimsPrincipal user,
            AddBusinessPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var command = new AddBusinessPhotoCommand(
                GetBusinessId(user),
                request.Kind,
                request.ImageUrl,
                request.PublicId,
                request.Caption);

            var response = await handler.HandleAsync(command, cancellationToken);

            return Results.Created($"/api/businesses/me/photos/{response.BusinessPhotoId.Value}", response);
        });

        meGroup.MapGet("/photos", async (
            ClaimsPrincipal user,
            GetMyBusinessPhotosQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new GetMyBusinessPhotosQuery(GetBusinessId(user)),
                cancellationToken);

            return Results.Ok(response);
        });

        meGroup.MapDelete("/photos/{id}", async (
            string id,
            ClaimsPrincipal user,
            DeleteBusinessPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new DeleteBusinessPhotoCommand(new BusinessPhotoId(id), GetBusinessId(user)),
                cancellationToken);

            return Results.NoContent();
        });
    }

    private static BusinessId GetBusinessId(ClaimsPrincipal user)
        => new(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
