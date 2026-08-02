using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Appointments.SendDueReminders;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.BackgroundServices;

/// <summary>
/// Revisa periódicamente si hay citas cuyo recordatorio de WhatsApp
/// ya debe salir. La lógica de negocio vive en
/// <see cref="SendDueRemindersCommandHandler"/>; esta clase solo aporta
/// el "cada cuánto".
/// </summary>
public sealed class AppointmentReminderBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ReminderPollingOptions _pollingOptions;
    private readonly ILogger<AppointmentReminderBackgroundService> _logger;

    public AppointmentReminderBackgroundService(
        IServiceScopeFactory scopeFactory,
        IOptions<ReminderPollingOptions> pollingOptions,
        ILogger<AppointmentReminderBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _pollingOptions = pollingOptions.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(_pollingOptions.PollIntervalSeconds));

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var handler = scope.ServiceProvider.GetRequiredService<SendDueRemindersCommandHandler>();
                await handler.HandleAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falló la revisión de recordatorios de citas pendientes");
            }
        }
    }
}
