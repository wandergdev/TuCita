using TuCita.Application.Abstractions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Professionals.RegisterProfessional;

/// <summary>
/// Se encarga de registrar un nuevo profesional.
/// </summary>
public sealed class RegisterProfessionalCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IIdentifierGenerator _identifierGenerator;

    public RegisterProfessionalCommandHandler(
        IProfessionalRepository professionalRepository,
        IIdentifierGenerator identifierGenerator)
    {
        _professionalRepository = professionalRepository
                                  ?? throw new ArgumentNullException(nameof(professionalRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
    }

    public async Task<RegisterProfessionalResponse> HandleAsync(
        RegisterProfessionalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var slug = Slug.Create(command.Slug);

        var slugExists = await _professionalRepository.ExistsBySlugAsync(
            slug.Value,
            cancellationToken);

        if (slugExists)
            throw new InvalidOperationException(
                "El slug ya se encuentra registrado.");

        var professional = Professional.Create(
            _identifierGenerator.NewProfessionalId(),
            command.FullName,
            Email.Create(command.Email),
            PhoneNumber.Create(command.PhoneNumber),
            slug,
            command.Category,
            AppointmentDuration.Create(command.AppointmentDuration),
            command.WorkingSchedule);

        await _professionalRepository.AddAsync(
            professional,
            cancellationToken);

        return new RegisterProfessionalResponse(
            professional.Id);
    }
}