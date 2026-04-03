using FluentValidation;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Employee.Application.Commands;


public sealed record UpdateEmployeeCommand(
    Guid TenantId,
    Guid EmployeeId,
    string Name,
    string Department,
    string Position) : ICommand;

public sealed class UpdateEmployeeCommandValidator : AbstractValidator<UpdateEmployeeCommand>
{
    public UpdateEmployeeCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Position).NotEmpty().MaximumLength(100);
    }
}

public sealed class UpdateEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(UpdateEmployeeCommand command, CancellationToken ct)
    {
        var employee = await repository.GetByIdAsync(command.EmployeeId, ct).ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure("Employee not found.", "EMPLOYEE_NOT_FOUND");
        }

        employee.Update(command.Name, command.Department, command.Position);
        repository.Update(employee);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Success();
    }
}

public sealed record DeleteEmployeeCommand(Guid TenantId, Guid EmployeeId) : ICommand;

public sealed class DeleteEmployeeCommandValidator : AbstractValidator<DeleteEmployeeCommand>
{
    public DeleteEmployeeCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}

public sealed class DeleteEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<DeleteEmployeeCommand, Result>
{
    public async Task<Result> Handle(DeleteEmployeeCommand command, CancellationToken ct)
    {
        var employee = await repository.GetByIdAsync(command.EmployeeId, ct).ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure("Employee not found.", "EMPLOYEE_NOT_FOUND");
        }

        employee.Delete();
        repository.Update(employee);
        await repository.SaveChangesAsync(ct).ConfigureAwait(false);

        return Result.Success();
    }
}
