using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.Photos.AddBusinessPhoto;

/// <summary>
/// Registra una foto de negocio ya subida a Cloudinary (portada o del
/// espacio). Entra pendiente de aprobación.
/// </summary>
public sealed class AddBusinessPhotoCommandHandler
{
    private const int MaxActiveCoverPhotos = 1;
    private const int MaxActiveSpacePhotos = 4;

    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessPhotoRepository _businessPhotoRepository;
    private readonly IIdentifierGenerator _identifierGenerator;

    public AddBusinessPhotoCommandHandler(
        IBusinessRepository businessRepository,
        IBusinessPhotoRepository businessPhotoRepository,
        IIdentifierGenerator identifierGenerator)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
    }

    public async Task<AddBusinessPhotoResponse> HandleAsync(
        AddBusinessPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var business = await _businessRepository.GetByIdAsync(command.BusinessId, cancellationToken);

        if (business is null)
            throw new NotFoundException("El negocio solicitado no existe.");

        var existingPhotos = await _businessPhotoRepository.GetByBusinessIdAsync(business.Id, cancellationToken);

        var activeCount = existingPhotos.Count(p =>
            p.Kind == command.Kind && p.Status != PortfolioPhotoStatus.Rejected);

        var limit = command.Kind == BusinessPhotoKind.Cover ? MaxActiveCoverPhotos : MaxActiveSpacePhotos;

        if (activeCount >= limit)
            throw new InvalidOperationException(
                command.Kind == BusinessPhotoKind.Cover
                    ? "Ya tienes una imagen de portada. Elimínala antes de subir otra."
                    : $"No puedes tener más de {MaxActiveSpacePhotos} fotos del espacio.");

        var photo = BusinessPhoto.Create(
            _identifierGenerator.NewBusinessPhotoId(),
            business.Id,
            command.Kind,
            command.ImageUrl,
            command.PublicId,
            command.Caption);

        await _businessPhotoRepository.AddAsync(photo, cancellationToken);

        return new AddBusinessPhotoResponse(photo.Id);
    }
}
