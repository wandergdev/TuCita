using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Options;

namespace TuCita.Application.Admin.Login;

/// <summary>
/// Autentica a la cuenta de administración (moderación de portafolio)
/// contra las credenciales configuradas y emite su token de acceso.
/// </summary>
public sealed class AdminLoginCommandHandler
{
    private readonly AdminOptions _adminOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IAuthTokenGenerator _authTokenGenerator;

    public AdminLoginCommandHandler(
        IOptions<AdminOptions> adminOptions,
        IPasswordHasher passwordHasher,
        IAuthTokenGenerator authTokenGenerator)
    {
        _adminOptions = adminOptions?.Value ?? throw new ArgumentNullException(nameof(adminOptions));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
        _authTokenGenerator = authTokenGenerator
            ?? throw new ArgumentNullException(nameof(authTokenGenerator));
    }

    public Task<AdminLoginResponse> HandleAsync(
        AdminLoginCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var isConfigured = !string.IsNullOrEmpty(_adminOptions.Username)
            && !string.IsNullOrEmpty(_adminOptions.PasswordHash);

        var isValidUsername = isConfigured
            && string.Equals(command.Username.Trim(), _adminOptions.Username, StringComparison.Ordinal);

        if (!isValidUsername || !_passwordHasher.Verify(command.Password, _adminOptions.PasswordHash))
            throw new UnauthorizedException("Credenciales inválidas.");

        var token = _authTokenGenerator.GenerateAdminToken(_adminOptions.Username);

        return Task.FromResult(new AdminLoginResponse(token));
    }
}
