namespace HrSaas.Contracts.Notifications;

using HrSaas.SharedKernel.Events;

public sealed record SendEmailNotificationCommand : IIntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid TenantId { get; init; }
    public DateTime OccurredAt { get; init; } = DateTime.UtcNow;
    public required string ToEmail { get; init; }
    public required string Subject { get; init; }
    public required string BodyHtml { get; init; }
    public string? BodyText { get; init; }
    public string? CorrelationId { get; init; }
}
