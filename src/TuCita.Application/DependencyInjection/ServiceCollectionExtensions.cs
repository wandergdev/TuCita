using Microsoft.Extensions.DependencyInjection;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Appointments.CancelAppointment;
using TuCita.Application.Appointments.CompleteAppointment;
using TuCita.Application.Appointments.GetAppointmentStats;
using TuCita.Application.Appointments.GetProfessionalAppointments;
using TuCita.Application.Appointments.MarkAppointmentAsNoShow;
using TuCita.Application.Appointments.SendDueReminders;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.Professionals.GetProfessionalPublicProfile;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.Professionals.RegisterProfessional;

namespace TuCita.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterProfessionalCommandHandler>();
        services.AddScoped<LoginProfessionalCommandHandler>();
        services.AddScoped<GetProfessionalPublicProfileQueryHandler>();
        services.AddScoped<GetAvailabilityQueryHandler>();
        services.AddScoped<BookAppointmentCommandHandler>();
        services.AddScoped<GetProfessionalAppointmentsQueryHandler>();
        services.AddScoped<GetAppointmentStatsQueryHandler>();
        services.AddScoped<CancelAppointmentCommandHandler>();
        services.AddScoped<CompleteAppointmentCommandHandler>();
        services.AddScoped<MarkAppointmentAsNoShowCommandHandler>();
        services.AddScoped<SendDueRemindersCommandHandler>();

        return services;
    }
}
