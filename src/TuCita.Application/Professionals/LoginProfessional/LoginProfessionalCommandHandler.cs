using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.LoginProfessional;

/// <summary>
/// Autentica a un profesional y emite su token de acceso.
/// </summary>
public sealed class LoginProfessionalCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;

    public LoginProfessionalCommandHandler(
        IProfessionalRepository professionalRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _passwordHasher = passwordHasher
            ?? throw new ArgumentNullException(nameof(passwordHasher));
        _authTokenGenerator = authTokenGenerator
            ?? throw new ArgumentNullException(nameof(authTokenGenerator));
    }

    public async Task<LoginProfessionalResponse> HandleAsync(
        LoginProfessionalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByEmailAsync(
            command.Email.Trim().ToLower(),
            cancellationToken);

        if (professional is null || !_passwordHasher.Verify(command.Password, professional.PasswordHash))
            throw new UnauthorizedException("Credenciales inválidas.");

        var token = _authTokenGenerator.Generate(professional);

        return new LoginProfessionalResponse(token, professional.Id, professional.Slug.Value);
    }
}
