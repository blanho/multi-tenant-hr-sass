using FluentValidation;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Employee.Application.Commands;

[Auditable(AuditAction.Delete, AuditCategory.Employee, Severity = AuditSeverity.High)]
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
    public async Task<Result> Handle(DeleteEmployeeCommand command, CancellationToken cancellationToken)
    {
        var employee = await repository.GetByIdAsync(command.EmployeeId, cancellationToken).ConfigureAwait(false);

        if (employee is null)
        {
            return Result.Failure("Employee not found.", "EMPLOYEE_NOT_FOUND");
        }

        employee.Delete();
        repository.Update(employee);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
