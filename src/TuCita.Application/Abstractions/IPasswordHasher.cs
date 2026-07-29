namespace TuCita.Application.Abstractions;

/// <summary>
/// Genera y verifica hashes de contraseñas.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
