using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.VerifyBusinessEmail;

/// <summary>
/// Confirma el correo de un negocio a partir del token que recibió por
/// correo al registrarse.
/// </summary>
public sealed class VerifyBusinessEmailCommandHandler
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VerifyBusinessEmailCommandHandler(
        IBusinessRepository businessRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task HandleAsync(
        VerifyBusinessEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Token))
            throw new NotFoundException("El enlace de verificación no es válido.");

        var business = await _businessRepository.GetByEmailVerificationTokenAsync(
            command.Token,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("El enlace de verificación no es válido.");

        if (business.EmailVerificationTokenExpiresAt is null
            || business.EmailVerificationTokenExpiresAt < _dateTimeProvider.Now)
            throw new ArgumentException("El enlace de verificación expiró. Solicita uno nuevo.");

        business.VerifyEmail();

        await _businessRepository.UpdateAsync(business, cancellationToken);
    }
}
