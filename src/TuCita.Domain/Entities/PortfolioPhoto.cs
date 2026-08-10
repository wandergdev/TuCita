using TuCita.Domain.Common;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Entities;

/// <summary>
/// Foto del trabajo de un profesional, subida directo a Cloudinary desde
/// el navegador. Entra en <see cref="PortfolioPhotoStatus.Pending"/> y solo
/// se muestra en los perfiles públicos una vez aprobada.
/// </summary>
public sealed class PortfolioPhoto : Entity<PortfolioPhotoId>
{
    private const int MaxCaptionLength = 200;

    public ProfessionalId ProfessionalId { get; private set; }

    public string ImageUrl { get; private set; }

    /// <summary>
    /// Identificador de Cloudinary para la imagen, necesario para poder
    /// borrarla del almacenamiento.
    /// </summary>
    public string PublicId { get; private set; }

    public string? Caption { get; private set; }

    public PortfolioPhotoStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private PortfolioPhoto(
        PortfolioPhotoId id,
        ProfessionalId professionalId,
        string imageUrl,
        string publicId,
        string? caption)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ProfessionalId = professionalId ?? throw new ArgumentNullException(nameof(professionalId));

        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("La URL de la imagen es obligatoria.", nameof(imageUrl));
        ImageUrl = imageUrl.Trim();

        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("El identificador de Cloudinary es obligatorio.", nameof(publicId));
        PublicId = publicId.Trim();

        var trimmedCaption = caption?.Trim();
        if (!string.IsNullOrEmpty(trimmedCaption) && trimmedCaption.Length > MaxCaptionLength)
            throw new ArgumentException(
                $"El texto no puede superar los {MaxCaptionLength} caracteres.",
                nameof(caption));

        Caption = string.IsNullOrEmpty(trimmedCaption) ? null : trimmedCaption;
        Status = PortfolioPhotoStatus.Pending;
        CreatedAt = DateTime.UtcNow;
    }

    public static PortfolioPhoto Create(
        PortfolioPhotoId id,
        ProfessionalId professionalId,
        string imageUrl,
        string publicId,
        string? caption = null)
    {
        return new PortfolioPhoto(id, professionalId, imageUrl, publicId, caption);
    }

    /// <summary>
    /// Reemplaza la imagen (ej. cuando el admin la descarga, la mejora y
    /// vuelve a subirla). No cambia el estado de moderación.
    /// </summary>
    public void ReplaceImage(string imageUrl, string publicId)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            throw new ArgumentException("La URL de la imagen es obligatoria.", nameof(imageUrl));

        if (string.IsNullOrWhiteSpace(publicId))
            throw new ArgumentException("El identificador de Cloudinary es obligatorio.", nameof(publicId));

        ImageUrl = imageUrl.Trim();
        PublicId = publicId.Trim();
    }

    public void Approve()
    {
        if (Status != PortfolioPhotoStatus.Pending)
            throw new InvalidOperationException("Solo se pueden aprobar fotos pendientes.");

        Status = PortfolioPhotoStatus.Approved;
    }

    public void Reject()
    {
        if (Status != PortfolioPhotoStatus.Pending)
            throw new InvalidOperationException("Solo se pueden rechazar fotos pendientes.");

        Status = PortfolioPhotoStatus.Rejected;
    }
}
