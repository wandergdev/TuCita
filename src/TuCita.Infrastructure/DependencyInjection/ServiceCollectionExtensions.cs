using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuCita.Application.Abstractions;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.Options;
using TuCita.Infrastructure.Persistence;
using TuCita.Infrastructure.Repositories;
using TuCita.Infrastructure.Services;

namespace TuCita.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<TuCitaDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateOnStart();

        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddSingleton<IIdentifierGenerator, UlidGenerator>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
