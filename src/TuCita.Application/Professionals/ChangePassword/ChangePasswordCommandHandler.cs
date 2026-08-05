using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ChangePassword;

/// <summary>
/// Cambia la contraseña del profesional autenticado, verificando primero
/// la contraseña actual.
/// </summary>
public sealed class ChangePasswordCommandHandler
{
    private const int MinPasswordLength = 8;

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPasswordHasher _passwordHasher;

    public ChangePasswordCommandHandler(
        IProfessionalRepository professionalRepository,
        IPasswordHasher passwordHasher)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _passwordHasher = passwordHasher
            ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public async Task HandleAsync(
        ChangePasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (string.IsNullOrWhiteSpace(command.NewPassword) || command.NewPassword.Length < MinPasswordLength)
            throw new ArgumentException($"La contraseña debe tener al menos {MinPasswordLength} caracteres.");

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional solicitado no existe.");

        if (!_passwordHasher.Verify(command.CurrentPassword, professional.PasswordHash))
            throw new ArgumentException("La contraseña actual no es correcta.");

        professional.ChangePassword(_passwordHasher.Hash(command.NewPassword));

        await _professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
