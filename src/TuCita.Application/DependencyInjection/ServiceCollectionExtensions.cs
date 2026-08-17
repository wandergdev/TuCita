using Microsoft.Extensions.DependencyInjection;
using TuCita.Application.Admin.BusinessPhotos.ApproveBusinessPhoto;
using TuCita.Application.Admin.BusinessPhotos.CreateAdminBusinessPhotoUploadSignature;
using TuCita.Application.Admin.BusinessPhotos.GetPendingBusinessPhotos;
using TuCita.Application.Admin.BusinessPhotos.RejectBusinessPhoto;
using TuCita.Application.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;
using TuCita.Application.Admin.Login;
using TuCita.Application.Admin.Portfolio.ApprovePortfolioPhoto;
using TuCita.Application.Admin.Portfolio.CreateAdminPortfolioUploadSignature;
using TuCita.Application.Admin.Portfolio.GetPendingPortfolioPhotos;
using TuCita.Application.Admin.Portfolio.RejectPortfolioPhoto;
using TuCita.Application.Admin.Portfolio.ReplacePortfolioPhotoImage;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Appointments.CancelAppointment;
using TuCita.Application.Appointments.CheckExistingAppointment;
using TuCita.Application.Appointments.CompleteAppointment;
using TuCita.Application.Appointments.GetAppointmentStats;
using TuCita.Application.Appointments.GetProfessionalAppointments;
using TuCita.Application.Appointments.MarkAppointmentAsNoShow;
using TuCita.Application.Appointments.SendDueReminders;
using TuCita.Application.Businesses.CreateBusinessInvite;
using TuCita.Application.Businesses.GetBusinessInvite;
using TuCita.Application.Businesses.GetBusinessPublicProfile;
using TuCita.Application.Businesses.GetMyRoster;
using TuCita.Application.Businesses.LoginBusiness;
using TuCita.Application.Businesses.Photos.AddBusinessPhoto;
using TuCita.Application.Businesses.Photos.CreateBusinessPhotoUploadSignature;
using TuCita.Application.Businesses.Photos.DeleteBusinessPhoto;
using TuCita.Application.Businesses.Photos.GetMyBusinessPhotos;
using TuCita.Application.Businesses.RegisterBusiness;
using TuCita.Application.Businesses.RemoveProfessional;
using TuCita.Application.Businesses.ResendBusinessVerificationEmail;
using TuCita.Application.Businesses.VerifyBusinessEmail;
using TuCita.Application.Professionals.ChangePassword;
using TuCita.Application.Professionals.ForgotPassword;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.Professionals.GetMyWorkingSchedule;
using TuCita.Application.Professionals.GetProfessionalPublicProfile;
using TuCita.Application.Professionals.JoinBusiness;
using TuCita.Application.Professionals.LoginProfessional;
using TuCita.Application.Professionals.Portfolio.AddPortfolioPhoto;
using TuCita.Application.Professionals.Portfolio.CreatePortfolioUploadSignature;
using TuCita.Application.Professionals.Portfolio.DeletePortfolioPhoto;
using TuCita.Application.Professionals.Portfolio.GetMyPortfolio;
using TuCita.Application.Professionals.RegisterProfessional;
using TuCita.Application.Professionals.ResendVerificationEmail;
using TuCita.Application.Professionals.ResetPassword;
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
        services.AddScoped<ForgotPasswordCommandHandler>();
        services.AddScoped<ResetPasswordCommandHandler>();
        services.AddScoped<ChangePasswordCommandHandler>();
        services.AddScoped<RegisterBusinessCommandHandler>();
        services.AddScoped<LoginBusinessCommandHandler>();
        services.AddScoped<VerifyBusinessEmailCommandHandler>();
        services.AddScoped<ResendBusinessVerificationEmailCommandHandler>();
        services.AddScoped<CreateBusinessInviteCommandHandler>();
        services.AddScoped<GetBusinessInviteQueryHandler>();
        services.AddScoped<GetMyRosterQueryHandler>();
        services.AddScoped<RemoveProfessionalCommandHandler>();
        services.AddScoped<JoinBusinessCommandHandler>();
        services.AddScoped<GetBusinessPublicProfileQueryHandler>();
        services.AddScoped<CreatePortfolioUploadSignatureQueryHandler>();
        services.AddScoped<AddPortfolioPhotoCommandHandler>();
        services.AddScoped<DeletePortfolioPhotoCommandHandler>();
        services.AddScoped<GetMyPortfolioQueryHandler>();
        services.AddScoped<GetPendingPortfolioPhotosQueryHandler>();
        services.AddScoped<ApprovePortfolioPhotoCommandHandler>();
        services.AddScoped<RejectPortfolioPhotoCommandHandler>();
        services.AddScoped<AdminLoginCommandHandler>();
        services.AddScoped<CreateAdminPortfolioUploadSignatureQueryHandler>();
        services.AddScoped<ReplacePortfolioPhotoImageCommandHandler>();
        services.AddScoped<CreateBusinessPhotoUploadSignatureQueryHandler>();
        services.AddScoped<AddBusinessPhotoCommandHandler>();
        services.AddScoped<DeleteBusinessPhotoCommandHandler>();
        services.AddScoped<GetMyBusinessPhotosQueryHandler>();
        services.AddScoped<GetPendingBusinessPhotosQueryHandler>();
        services.AddScoped<ApproveBusinessPhotoCommandHandler>();
        services.AddScoped<RejectBusinessPhotoCommandHandler>();
        services.AddScoped<CreateAdminBusinessPhotoUploadSignatureQueryHandler>();
        services.AddScoped<ReplaceBusinessPhotoImageCommandHandler>();

        // Nombres iguales a los de Professionals.ForgotPassword/ResetPassword;
        // se referencian con el namespace completo para no chocar con el
        // "using" de esos (ver arriba).
        services.AddScoped<TuCita.Application.Businesses.ForgotPassword.ForgotPasswordCommandHandler>();
        services.AddScoped<TuCita.Application.Businesses.ResetPassword.ResetPasswordCommandHandler>();

        return services;
    }
}
