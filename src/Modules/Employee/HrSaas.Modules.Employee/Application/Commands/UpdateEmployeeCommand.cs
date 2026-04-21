using FluentValidation;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Employee.Application.Commands;

[Auditable(AuditAction.Update, AuditCategory.Employee, Severity = AuditSeverity.Medium)]
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
    public async Task<Result> Handle(UpdateEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(command.EmployeeId, cancellationToken).ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure("Employee not found.", "EMPLOYEE_NOT_FOUND");
        }

        employee.Update(command.Name, command.Department, command.Position);
        repository.Update(employee);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
