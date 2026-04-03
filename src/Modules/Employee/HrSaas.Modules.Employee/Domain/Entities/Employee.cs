using HrSaas.Modules.Employee.Domain.Events;
using HrSaas.Modules.Employee.Domain.ValueObjects;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Employee.Domain.Entities;

public sealed class Employee : BaseEntity
{
    public string Name { get; private set; } = default!;

    public Department Department { get; private set; } = default!;

    public Position Position { get; private set; } = default!;

    public string Email { get; private set; } = default!;

    public bool IsActive { get; private set; } = true;

    public Guid? UserId { get; private set; }

    private Employee() { }

    public static Employee Create(
        Guid tenantId,
        string name,
        string department,
        string position,
        string email,
        Guid? userId = null)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));

        var employee = new Employee
        {
            TenantId = tenantId,
            Name = Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim(), 200, nameof(name)),
            Department = Department.Create(department),
            Position = Position.Create(position),
            Email = ValidateEmail(email),
            IsActive = true,
            UserId = userId
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(
            tenantId, employee.Id, name, department, position));

        return employee;
    }

    public void Update(string name, string department, string position)
    {
        Name = Guard.MaxLength(Guard.NotNullOrWhiteSpace(name, nameof(name)).Trim(), 200, nameof(name));
        Department = Department.Create(department);
        Position = Position.Create(position);
        Touch();

        AddDomainEvent(new EmployeeUpdatedEvent(TenantId, Id, name, department, position));
    }

    public void Deactivate()
    {
        if (!IsActive)
        {
            return;
        }

        IsActive = false;
        Touch();
        AddDomainEvent(new EmployeeDeactivatedEvent(TenantId, Id));
    }

    public void Activate()
    {
        if (IsActive)
        {
            return;
        }

        IsActive = true;
        Touch();
    }

    public override void Delete()
    {
        base.Delete();
        AddDomainEvent(new EmployeeDeletedEvent(TenantId, Id));
    }

    public void LinkToUser(Guid userId)
    {
        Guard.NotEmpty(userId, nameof(userId));
        UserId = userId;
        Touch();
    }

    private static string ValidateEmail(string email)
    {
        Guard.NotNullOrWhiteSpace(email, nameof(email));
        var trimmed = email.Trim().ToLowerInvariant();
        if (!trimmed.Contains('@') || trimmed.IndexOf('@') == 0 || trimmed.LastIndexOf('.') < trimmed.IndexOf('@'))
        {
            throw new ArgumentException("Email must be a valid email address.", nameof(email));
        }

        return trimmed;
    }
}

