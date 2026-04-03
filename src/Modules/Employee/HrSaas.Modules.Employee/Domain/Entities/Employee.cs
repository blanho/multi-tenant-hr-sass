using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;
using HrSaas.Modules.Employee.Domain.Events;
using HrSaas.Modules.Employee.Domain.ValueObjects;

namespace HrSaas.Modules.Employee.Domain.Entities;

public sealed class Employee : BaseEntity
{
    public string Name { get; private set; } = default!;

    public Department Department { get; private set; } = default!;

    public Position Position { get; private set; } = default!;

    public string Email { get; private set; } = default!;

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
        if (tenantId == Guid.Empty)
            throw new DomainException("TenantId is required.");

        var employee = new Employee
        {
            TenantId = tenantId,
            Name = ValidateName(name),
            Department = Department.Create(department),
            Position = Position.Create(position),
            Email = ValidateEmail(email),
            UserId = userId
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(
            tenantId, employee.Id, name, department, position));

        return employee;
    }

    public void Update(string name, string department, string position)
    {
        Name = ValidateName(name);
        Department = Department.Create(department);
        Position = Position.Create(position);
        Touch();

        AddDomainEvent(new EmployeeUpdatedEvent(TenantId, Id, name));
    }

    public override void Delete()
    {
        base.Delete();
        AddDomainEvent(new EmployeeDeletedEvent(TenantId, Id));
    }

    public void LinkToUser(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new DomainException("UserId cannot be empty.");

        UserId = userId;
        Touch();
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Employee name is required.");

        if (name.Length > 200)
            throw new DomainException("Employee name cannot exceed 200 characters.");

        return name.Trim();
    }

    private static string ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Employee email is required.");

        return email.Trim().ToLowerInvariant();
    }
}
