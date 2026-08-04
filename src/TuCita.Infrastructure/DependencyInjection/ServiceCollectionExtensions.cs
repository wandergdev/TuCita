using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TuCita.Application.Abstractions;
using TuCita.Application.Options;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.BackgroundServices;
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

        services.AddOptions<EmailOptions>()
            .Bind(configuration.GetSection(EmailOptions.SectionName));

        services.AddOptions<TwilioOptions>()
            .Bind(configuration.GetSection(TwilioOptions.SectionName));

        services.AddOptions<ReminderOptions>()
            .Bind(configuration.GetSection(ReminderOptions.SectionName));

        services.AddOptions<ReminderPollingOptions>()
            .Bind(configuration.GetSection(ReminderPollingOptions.SectionName));

        services.AddScoped<IProfessionalRepository, ProfessionalRepository>();
        services.AddScoped<IAppointmentRepository, AppointmentRepository>();
        services.AddSingleton<IIdentifierGenerator, UlidGenerator>();
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddScoped<IAuthTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddHttpClient<IEmailSender, BrevoEmailSender>(client =>
        {
            client.BaseAddress = new Uri("https://api.brevo.com/v3/");
        });
        services.AddScoped<IWhatsAppSender, TwilioWhatsAppSender>();

        services.AddHostedService<AppointmentReminderBackgroundService>();

        return services;
    }
}
