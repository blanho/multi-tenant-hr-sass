#!/usr/bin/env python3
"""Fix test files to match actual Employee entity API."""
import os

ROOT = "/Users/macbook/Desktop/multi-tenant-sass"

def write(rel_path, content):
    full = os.path.join(ROOT, rel_path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  {rel_path}")

write("tests/HrSaas.Modules.Employee.UnitTests/Domain/EmployeeAggregateTests.cs", """\
using Bogus;
using FluentAssertions;
using HrSaas.Modules.Employee.Domain.Entities;
using HrSaas.Modules.Employee.Domain.Events;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeAggregateTests
{
    private static readonly Faker Faker = new();
    private readonly Guid _tenantId = Guid.NewGuid();

    [Fact]
    public void Create_WithValidData_ShouldCreateEmployee()
    {
        var employee = Employee.Create(
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
        var employee = Employee.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeCreatedEvent);
    }

    [Fact]
    public void Create_WithEmptyName_ShouldThrow()
    {
        var act = () => Employee.Create(_tenantId, string.Empty, "Engineering", "Developer", "john@example.com");

        act.Should().Throw<Exception>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithEmptyTenantId_ShouldThrow()
    {
        var act = () => Employee.Create(Guid.Empty, "John Doe", "Engineering", "Developer", "john@example.com");

        act.Should().Throw<Exception>().WithMessage("*TenantId*");
    }

    [Fact]
    public void Delete_ShouldMarkAsDeleted()
    {
        var employee = Employee.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");
        employee.ClearDomainEvents();

        employee.Delete();

        employee.IsDeleted.Should().BeTrue();
        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeDeletedEvent);
    }

    [Fact]
    public void Update_ShouldRaiseUpdatedEvent()
    {
        var employee = Employee.Create(_tenantId, "John Doe", "Engineering", "Developer", "john@example.com");
        employee.ClearDomainEvents();

        employee.Update("Jane Doe", "Product", "PM");

        employee.DomainEvents.Should().ContainSingle(e => e is EmployeeUpdatedEvent);
    }
}
""")

write("tests/HrSaas.Modules.Employee.UnitTests/Domain/EmployeeValueObjectTests.cs", """\
using FluentAssertions;
using HrSaas.Modules.Employee.Domain.ValueObjects;

namespace HrSaas.Modules.Employee.UnitTests.Domain;

public sealed class EmployeeValueObjectTests
{
    [Theory]
    [InlineData("Engineering")]
    [InlineData("Human Resources")]
    [InlineData("Finance")]
    public void Department_Create_WithValidName_ShouldSucceed(string name)
    {
        var dept = Department.Create(name);
        dept.Name.Should().Be(name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Department_Create_WithInvalidName_ShouldThrow(string name)
    {
        var act = () => Department.Create(name);
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Department_SameValues_ShouldBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Engineering");
        d1.Should().Be(d2);
    }

    [Fact]
    public void Department_DifferentValues_ShouldNotBeEqual()
    {
        var d1 = Department.Create("Engineering");
        var d2 = Department.Create("Finance");
        d1.Should().NotBe(d2);
    }
}
""")

write("tests/HrSaas.Modules.Employee.UnitTests/Application/CreateEmployeeCommandHandlerTests.cs", """\
using FluentAssertions;
using HrSaas.Modules.Employee.Application.Commands;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.Modules.Employee.Domain.Entities;
using NSubstitute;

namespace HrSaas.Modules.Employee.UnitTests.Application;

public sealed class CreateEmployeeCommandHandlerTests
{
    private readonly IEmployeeRepository _repository = Substitute.For<IEmployeeRepository>();
    private readonly CreateEmployeeCommandHandler _handler;

    public CreateEmployeeCommandHandlerTests()
    {
        _handler = new CreateEmployeeCommandHandler(_repository);
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateAndReturnId()
    {
        var tenantId = Guid.NewGuid();
        var command = new CreateEmployeeCommand(tenantId, "Jane Doe", "Engineering", "Developer", "jane@example.com");

        _repository.SaveChangesAsync(Arg.Any<CancellationToken>()).Returns(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);
        await _repository.Received(1).AddAsync(Arg.Any<Employee>(), Arg.Any<CancellationToken>());
        await _repository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_EmptyTenantId_ShouldFail()
    {
        var command = new CreateEmployeeCommand(Guid.Empty, "Jane Doe", "Engineering", "Developer", "jane@example.com");

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<Exception>();
    }
}
""")

print("Test files fixed.")
