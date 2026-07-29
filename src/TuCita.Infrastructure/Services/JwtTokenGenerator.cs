using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using TuCita.Application.Abstractions;
using TuCita.Domain.Entities;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

public sealed class JwtTokenGenerator : IAuthTokenGenerator
{
    private readonly JwtOptions _options;

    public JwtTokenGenerator(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public string Generate(Professional professional)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, professional.Id.Value),
            new Claim(ClaimTypes.Email, professional.Email.Value),
            new Claim(ClaimTypes.Name, professional.FullName)
        };

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
