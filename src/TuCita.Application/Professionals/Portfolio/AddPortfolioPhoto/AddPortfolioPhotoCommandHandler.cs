using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.Portfolio.AddPortfolioPhoto;

/// <summary>
/// Registra una foto ya subida a Cloudinary como parte del portafolio del
/// profesional. Entra pendiente de aprobación.
/// </summary>
public sealed class AddPortfolioPhotoCommandHandler
{
    private const int MaxActivePhotos = 2;

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IIdentifierGenerator _identifierGenerator;

    public AddPortfolioPhotoCommandHandler(
        IProfessionalRepository professionalRepository,
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IIdentifierGenerator identifierGenerator)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
    }

    public async Task<AddPortfolioPhotoResponse> HandleAsync(
        AddPortfolioPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional solicitado no existe.");

        var existingPhotos = await _portfolioPhotoRepository.GetByProfessionalIdAsync(
            professional.Id,
            cancellationToken);

        var activePhotoCount = existingPhotos.Count(p => p.Status != PortfolioPhotoStatus.Rejected);
        if (activePhotoCount >= MaxActivePhotos)
            throw new InvalidOperationException(
                $"No puedes tener más de {MaxActivePhotos} fotos en tu portafolio.");

        var photo = PortfolioPhoto.Create(
            _identifierGenerator.NewPortfolioPhotoId(),
            professional.Id,
            command.ImageUrl,
            command.PublicId,
            command.Caption);

        await _portfolioPhotoRepository.AddAsync(photo, cancellationToken);

        return new AddPortfolioPhotoResponse(photo.Id);
    }
}
