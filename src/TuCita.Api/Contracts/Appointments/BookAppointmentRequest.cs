namespace TuCita.Api.Contracts.Appointments;

public sealed record BookAppointmentRequest(
    string CustomerName,
    string CustomerPhone,
    DateOnly Date,
    TimeOnly StartTime);
