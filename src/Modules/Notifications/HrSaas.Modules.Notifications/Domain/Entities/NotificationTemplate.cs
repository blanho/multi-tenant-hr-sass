using HrSaas.Modules.Notifications.Domain.Enums;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Notifications.Domain.Entities;

public sealed class NotificationTemplate : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public NotificationChannel Channel { get; private set; }
    public NotificationCategory Category { get; private set; }
    public string SubjectTemplate { get; private set; } = string.Empty;
    public string BodyTemplate { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
    public string? SamplePayload { get; private set; }

    private NotificationTemplate() { }

    public static NotificationTemplate Create(
        Guid tenantId,
        string name,
        string slug,
        NotificationChannel channel,
        NotificationCategory category,
        string subjectTemplate,
        string bodyTemplate,
        string? description = null,
        string? samplePayload = null)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(slug, nameof(slug));
        Guard.NotNullOrWhiteSpace(subjectTemplate, nameof(subjectTemplate));
        Guard.NotNullOrWhiteSpace(bodyTemplate, nameof(bodyTemplate));

        return new NotificationTemplate
        {
            TenantId = tenantId,
            Name = name,
            Slug = slug.ToLowerInvariant().Replace(' ', '-'),
            Channel = channel,
            Category = category,
            SubjectTemplate = subjectTemplate,
            BodyTemplate = bodyTemplate,
            IsActive = true,
            Description = description,
            SamplePayload = samplePayload
        };
    }

    public void Update(
        string name,
        string subjectTemplate,
        string bodyTemplate,
        string? description = null)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(subjectTemplate, nameof(subjectTemplate));
        Guard.NotNullOrWhiteSpace(bodyTemplate, nameof(bodyTemplate));

        Name = name;
        SubjectTemplate = subjectTemplate;
        BodyTemplate = bodyTemplate;
        Description = description;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public string RenderSubject(IDictionary<string, string> variables) =>
        InterpolateTemplate(SubjectTemplate, variables);

    public string RenderBody(IDictionary<string, string> variables) =>
        InterpolateTemplate(BodyTemplate, variables);

    private static string InterpolateTemplate(string template, IDictionary<string, string> variables)
    {
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value, StringComparison.OrdinalIgnoreCase);
        }
        return result;
    }
}
