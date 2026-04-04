using FluentValidation;
using HrSaas.Modules.Notifications.Domain.Entities;
using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.Modules.Notifications.Domain.Repositories;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Notifications.Application.Commands;

public sealed record UpdateUserPreferencesCommand(
    Guid TenantId,
    Guid UserId,
    NotificationChannel Channel,
    NotificationCategory Category,
    bool IsEnabled,
    DigestFrequency DigestFrequency = DigestFrequency.Immediate,
    TimeOnly? QuietHoursStart = null,
    TimeOnly? QuietHoursEnd = null,
    string? TimeZone = null) : ICommand<Guid>;

public sealed class UpdateUserPreferencesCommandValidator : AbstractValidator<UpdateUserPreferencesCommand>
{
    public UpdateUserPreferencesCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.TimeZone)
            .Must(tz => tz is null || TimeZoneInfo.GetSystemTimeZones().Any(t => t.Id == tz))
            .WithMessage("Invalid timezone identifier.");
        RuleFor(x => x.QuietHoursEnd)
            .NotNull()
            .When(x => x.QuietHoursStart.HasValue)
            .WithMessage("QuietHoursEnd is required when QuietHoursStart is set.");
    }
}

public sealed class UpdateUserPreferencesCommandHandler(
    IUserNotificationPreferenceRepository repository)
    : IRequestHandler<UpdateUserPreferencesCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        UpdateUserPreferencesCommand command,
        CancellationToken ct)
    {
        var existing = await repository.GetAsync(
            command.UserId, command.Channel, command.Category, ct).ConfigureAwait(false);

        if (existing is not null)
        {
            existing.UpdatePreference(
                command.IsEnabled,
                command.DigestFrequency,
                command.QuietHoursStart,
                command.QuietHoursEnd,
                command.TimeZone);

            repository.Update(existing);
            await repository.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result<Guid>.Success(existing.Id);
        }

        var preference = UserNotificationPreference.Create(
            command.TenantId,
            command.UserId,
            command.Channel,
            command.Category,
            command.IsEnabled,
            command.DigestFrequency,
            command.QuietHoursStart,
            command.QuietHoursEnd,
            command.TimeZone);

        await repository.AddAsync(preference, ct).ConfigureAwait(false);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result<Guid>.Success(preference.Id);
    }
}
