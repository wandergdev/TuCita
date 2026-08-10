using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.LoginBusiness;

/// <summary>
/// Autentica a un negocio y emite su token de acceso.
/// </summary>
public sealed class LoginBusinessCommandHandler
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;

    public LoginBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _passwordHasher = passwordHasher
            ?? throw new ArgumentNullException(nameof(passwordHasher));
        _authTokenGenerator = authTokenGenerator
            ?? throw new ArgumentNullException(nameof(authTokenGenerator));
    }

    public async Task<LoginBusinessResponse> HandleAsync(
        LoginBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var business = await _businessRepository.GetByEmailAsync(
            command.Email.Trim().ToLower(),
            cancellationToken);

        if (business is null || !_passwordHasher.Verify(command.Password, business.PasswordHash))
            throw new UnauthorizedException("Credenciales inválidas.");

        if (!business.EmailVerified)
            throw new ForbiddenException("Debes verificar tu correo antes de iniciar sesión.");

        var token = _authTokenGenerator.Generate(business);

        return new LoginBusinessResponse(token, business.Id, business.Slug.Value, business.Name);
    }
}
