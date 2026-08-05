using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ResetPassword;

/// <summary>
/// Restablece la contraseña de un profesional a partir del token que
/// recibió por correo.
/// </summary>
public sealed class ResetPasswordCommandHandler
{
    private const int MinPasswordLength = 8;

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResetPasswordCommandHandler(
        IProfessionalRepository professionalRepository,
        IPasswordHasher passwordHasher,
        IDateTimeProvider dateTimeProvider)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _passwordHasher = passwordHasher
            ?? throw new ArgumentNullException(nameof(passwordHasher));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task HandleAsync(
        ResetPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.Token))
            throw new NotFoundException("El enlace para restablecer tu contraseña no es válido.");

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < MinPasswordLength)
            throw new ArgumentException($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");

        var professional = await _professionalRepository.GetByPasswordResetTokenAsync(
            command.Token,
            cancellationToken);

        if (professional is null)
            throw new NotFoundException("El enlace para restablecer tu contraseña no es válido.");

        if (professional.PasswordResetTokenExpiresAt is null
            || professional.PasswordResetTokenExpiresAt < _dateTimeProvider.Now)
            throw new ArgumentException("El enlace para restablecer tu contraseña expiró. Solicita uno nuevo.");

        professional.ResetPassword(_passwordHasher.Hash(command.NewPassword));

        await _professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
