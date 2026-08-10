using TuCita.Domain.Common;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Entities;

/// <summary>
/// Invitación de un solo uso que el dueño de un negocio genera para que
/// un profesional se registre ya asociado a ese negocio.
/// </summary>
public sealed class BusinessInvite : Entity<BusinessInviteId>
{
    public BusinessId BusinessId { get; private set; }

    public string Token { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    public DateTime? UsedAt { get; private set; }

    private BusinessInvite(
        BusinessInviteId id,
        BusinessId businessId,
        string token,
        DateTime expiresAt)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        BusinessId = businessId ?? throw new ArgumentNullException(nameof(businessId));

        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("El token de invitación es obligatorio.", nameof(token));
        Token = token;

        ExpiresAt = expiresAt;
    }

    public static BusinessInvite Create(
        BusinessInviteId id,
        BusinessId businessId,
        string token,
        DateTime expiresAt)
    {
        return new BusinessInvite(id, businessId, token, expiresAt);
    }

    public bool IsValid(DateTime now) => UsedAt is null && ExpiresAt >= now;

    /// <summary>
    /// Marca la invitación como usada. Falla si ya se usó o venció.
    /// </summary>
    public void Consume(DateTime now)
    {
        if (UsedAt is not null)
            throw new InvalidOperationException("Esta invitación ya fue utilizada.");

        if (ExpiresAt < now)
            throw new InvalidOperationException("Esta invitación venció.");

        UsedAt = now;
    }
}
