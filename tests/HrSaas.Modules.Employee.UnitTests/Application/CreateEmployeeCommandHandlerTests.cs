using FluentAssertions;
using HrSaas.Modules.Employee.Application.Commands;
using HrSaas.Modules.Employee.Application.Interfaces;
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
        await _repository.Received(1).AddAsync(Arg.Any<EmployeeEntity>(), Arg.Any<CancellationToken>());
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
