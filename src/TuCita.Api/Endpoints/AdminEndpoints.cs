using TuCita.Api.Contracts.Admin;
using TuCita.Application.Admin.BusinessPhotos.ApproveBusinessPhoto;
using TuCita.Application.Admin.BusinessPhotos.CreateAdminBusinessPhotoUploadSignature;
using TuCita.Application.Admin.BusinessPhotos.GetPendingBusinessPhotos;
using TuCita.Application.Admin.BusinessPhotos.RejectBusinessPhoto;
using TuCita.Application.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;
using TuCita.Application.Admin.Login;
using TuCita.Application.Admin.Portfolio.ApprovePortfolioPhoto;
using TuCita.Application.Admin.Portfolio.CreateAdminPortfolioUploadSignature;
using TuCita.Application.Admin.Portfolio.GetPendingPortfolioPhotos;
using TuCita.Application.Admin.Portfolio.RejectPortfolioPhoto;
using TuCita.Application.Admin.Portfolio.ReplacePortfolioPhotoImage;
using TuCita.Domain.Identifiers;

namespace TuCita.Api.Endpoints;

/// <summary>
/// Login y moderación de portafolio para la cuenta de administración
/// (login real vía JWT, política "AdminOnly" — ver Program.cs).
/// </summary>
public static class AdminEndpoints
{
    public static void MapAdminEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/admin/login", async (
            AdminLoginRequest request,
            AdminLoginCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new AdminLoginCommand(request.Username, request.Password),
                cancellationToken);

            return Results.Ok(response);
        }).WithTags("Admin");

        var group = app.MapGroup("/api/admin/portfolio")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        group.MapGet("/pending", async (
            GetPendingPortfolioPhotosQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(new GetPendingPortfolioPhotosQuery(), cancellationToken);

            return Results.Ok(response);
        });

        group.MapPost("/{id}/approve", async (
            string id,
            ApprovePortfolioPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ApprovePortfolioPhotoCommand(new PortfolioPhotoId(id)),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/{id}/reject", async (
            string id,
            RejectPortfolioPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new RejectPortfolioPhotoCommand(new PortfolioPhotoId(id)),
                cancellationToken);

            return Results.NoContent();
        });

        group.MapPost("/{id}/upload-signature", async (
            string id,
            CreateAdminPortfolioUploadSignatureQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CreateAdminPortfolioUploadSignatureQuery(new PortfolioPhotoId(id)),
                cancellationToken);

            return Results.Ok(response);
        });

        group.MapPut("/{id}/image", async (
            string id,
            ReplacePortfolioPhotoImageRequest request,
            ReplacePortfolioPhotoImageCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ReplacePortfolioPhotoImageCommand(new PortfolioPhotoId(id), request.ImageUrl, request.PublicId),
                cancellationToken);

            return Results.NoContent();
        });

        var businessPhotosGroup = app.MapGroup("/api/admin/business-photos")
            .WithTags("Admin")
            .RequireAuthorization("AdminOnly");

        businessPhotosGroup.MapGet("/pending", async (
            GetPendingBusinessPhotosQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(new GetPendingBusinessPhotosQuery(), cancellationToken);

            return Results.Ok(response);
        });

        businessPhotosGroup.MapPost("/{id}/approve", async (
            string id,
            ApproveBusinessPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ApproveBusinessPhotoCommand(new BusinessPhotoId(id)),
                cancellationToken);

            return Results.NoContent();
        });

        businessPhotosGroup.MapPost("/{id}/reject", async (
            string id,
            RejectBusinessPhotoCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new RejectBusinessPhotoCommand(new BusinessPhotoId(id)),
                cancellationToken);

            return Results.NoContent();
        });

        businessPhotosGroup.MapPost("/{id}/upload-signature", async (
            string id,
            CreateAdminBusinessPhotoUploadSignatureQueryHandler handler,
            CancellationToken cancellationToken) =>
        {
            var response = await handler.HandleAsync(
                new CreateAdminBusinessPhotoUploadSignatureQuery(new BusinessPhotoId(id)),
                cancellationToken);

            return Results.Ok(response);
        });

        businessPhotosGroup.MapPut("/{id}/image", async (
            string id,
            ReplacePortfolioPhotoImageRequest request,
            ReplaceBusinessPhotoImageCommandHandler handler,
            CancellationToken cancellationToken) =>
        {
            await handler.HandleAsync(
                new ReplaceBusinessPhotoImageCommand(new BusinessPhotoId(id), request.ImageUrl, request.PublicId),
                cancellationToken);

            return Results.NoContent();
        });
    }
}
