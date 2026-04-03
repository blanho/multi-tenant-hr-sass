using Bogus;
using FluentAssertions;
using HrSaas.Modules.Employee.Domain.Events;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeAggregateTests
{
    private static readonly Faker Faker = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateEmployee()
    {
        var employee = EmployeeEntity.Create(
            _tenantId,
            Faker.Name.FullName(),
            Faker.Commerce.Department(),
            Faker.Name.JobTitle(),
            Faker.Internet.Email());

        employee.Should().NotBeNull();
        employee.TenantId.Should().Be(_tenantId);
        employee.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldRaiseEmployeeCreatedEvent()
    {
        var employee = EmployeeEntity.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => EmployeeEntity.Create(_tenantId, string.Empty, "Engineering", "Developer", "john@example.com");

        act.Should().Throw<Exception>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrow()
    {
        var act = () => EmployeeEntity.Create(Guid.Empty, "John Doe", "Engineering", "Developer", "john@example.com");

        act.Should().Throw<Exception>().WithMessage("*TenantId*");
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var employee = EmployeeEntity.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");
        employee.ClearDomainEvents();

        employee.Delete();

        employee.IsDeleted.Should().BeTrue();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeDeletedEvent);
    }

    [Fact]
    public void Update_ShouldRaiseUpdatedEvent()
    {
        var employee = EmployeeEntity.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");
        employee.ClearDomainEvents();

        employee.Update("Jane Doe", "Product", "PM");

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeUpdatedEvent);
    }
}
