using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.ResetPassword;

/// <summary>
/// Restablece la contraseña de un negocio a partir del token que
/// recibió por correo.
/// </summary>
public sealed class ResetPasswordCommandHandler
{
    private const int MinPasswordLength = 8;

    private readonly IBusinessRepository _businessRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ResetPasswordCommandHandler(
        IBusinessRepository businessRepository,
        IPasswordHasher passwordHasher,
        IDateTimeProvider dateTimeProvider)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
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

        var business = await _businessRepository.GetByPasswordResetTokenAsync(
            command.Token,
            cancellationToken);

        if (business is null)
            throw new NotFoundException("El enlace para restablecer tu contraseña no es válido.");

        if (business.PasswordResetTokenExpiresAt is null
            || business.PasswordResetTokenExpiresAt < _dateTimeProvider.Now)
            throw new ArgumentException("El enlace para restablecer tu contraseña expiró. Solicita uno nuevo.");

        business.ResetPassword(_passwordHasher.Hash(command.NewPassword));

        await _businessRepository.UpdateAsync(business, cancellationToken);
    }
}
