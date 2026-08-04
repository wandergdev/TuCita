using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.VerifyEmail;

/// <summary>
/// Confirma el correo de un profesional a partir del token que
/// recibió por correo al registrarse.
/// </summary>
public sealed class VerifyEmailCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public VerifyEmailCommandHandler(
        IProfessionalRepository professionalRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task HandleAsync(
        VerifyEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Token))
            throw new NotFoundException("El enlace de verificación no es válido.");

        var professional = await _professionalRepository.GetByEmailVerificationTokenAsync(
            command.Token,
            cancellationToken);

        if (professional is null)
            throw new NotFoundException("El enlace de verificación no es válido.");

        if (professional.EmailVerificationTokenExpiresAt is null
            || professional.EmailVerificationTokenExpiresAt < _dateTimeProvider.Now)
            throw new ArgumentException("El enlace de verificación expiró. Solicita uno nuevo.");

        professional.VerifyEmail();

        await _professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
