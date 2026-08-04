using Microsoft.Extensions.DependencyInjection;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Appointments.CancelAppointment;
using TuCita.Application.Appointments.CheckExistingAppointment;
using TuCita.Application.Appointments.CompleteAppointment;
using TuCita.Application.Appointments.GetAppointmentStats;
using TuCita.Application.Appointments.GetProfessionalAppointments;
using TuCita.Application.Appointments.MarkAppointmentAsNoShow;
using TuCita.Application.Appointments.SendDueReminders;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.Professionals.GetMyWorkingSchedule;
using TuCita.Application.Professionals.GetProfessionalPublicProfile;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.Professionals.RegisterProfessional;
using TuCita.Application.Professionals.ResendVerificationEmail;
using TuCita.Application.Professionals.ScheduleExceptions.CreateScheduleException;
using TuCita.Application.Professionals.ScheduleExceptions.DeleteScheduleException;
using TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;
using TuCita.Application.Professionals.UpdateWorkingSchedule;
using TuCita.Application.Professionals.VerifyEmail;

namespace TuCita.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<RegisterProfessionalCommandHandler>();
        services.AddScoped<LoginProfessionalCommandHandler>();
        services.AddScoped<VerifyEmailCommandHandler>();
        services.AddScoped<ResendVerificationEmailCommandHandler>();
        services.AddScoped<GetProfessionalPublicProfileQueryHandler>();
        services.AddScoped<GetAvailabilityQueryHandler>();
        services.AddScoped<BookAppointmentCommandHandler>();
        services.AddScoped<CheckExistingAppointmentQueryHandler>();
        services.AddScoped<GetProfessionalAppointmentsQueryHandler>();
        services.AddScoped<GetAppointmentStatsQueryHandler>();
        services.AddScoped<CancelAppointmentCommandHandler>();
        services.AddScoped<CompleteAppointmentCommandHandler>();
        services.AddScoped<MarkAppointmentAsNoShowCommandHandler>();
        services.AddScoped<SendDueRemindersCommandHandler>();
        services.AddScoped<UpdateWorkingScheduleCommandHandler>();
        services.AddScoped<GetMyWorkingScheduleQueryHandler>();
        services.AddScoped<CreateScheduleExceptionCommandHandler>();
        services.AddScoped<GetScheduleExceptionsQueryHandler>();
        services.AddScoped<DeleteScheduleExceptionCommandHandler>();

        return services;
    }
}
