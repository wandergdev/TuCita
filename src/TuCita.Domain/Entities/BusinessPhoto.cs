using TuCita.Domain.Common;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Entities;

/// <summary>
/// Foto de un negocio (portada del hero o foto del espacio), subida
/// directo a Cloudinary desde el navegador del dueño. Entra en
/// <see cref="PortfolioPhotoStatus.Pending"/> y solo se muestra en la
/// página pública una vez aprobada.
/// </summary>
public sealed class BusinessPhoto : Entity<BusinessPhotoId>
{
    private const int MaxCaptionLength = 200;

    public BusinessId BusinessId { get; private set; }

    public BusinessPhotoKind Kind { get; private set; }

    public string ImageUrl { get; private set; }

    /// <summary>
    /// Identificador de Cloudinary para la imagen, necesario para poder
    /// borrarla del almacenamiento.
    /// </summary>
    public string PublicId { get; private set; }

    public string? Caption { get; private set; }

    public PortfolioPhotoStatus Status { get; private set; }

    public DateTime CreatedAt { get; private set; }

    private BusinessPhoto(
        BusinessPhotoId id,
        BusinessId businessId,
        BusinessPhotoKind kind,
        string imageUrl,
        string publicId,
        string? caption)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        BusinessId = businessId ?? throw new ArgumentNullException(nameof(businessId));
        Kind = kind;

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

    public static BusinessPhoto Create(
        BusinessPhotoId id,
        BusinessId businessId,
        BusinessPhotoKind kind,
        string imageUrl,
        string publicId,
        string? caption = null)
    {
        return new BusinessPhoto(id, businessId, kind, imageUrl, publicId, caption);
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
}
