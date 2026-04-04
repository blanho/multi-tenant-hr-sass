using FluentValidation;
using HrSaas.Modules.Employee.Application.DTOs;
using HrSaas.Modules.Employee.Application.Interfaces;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;
using EmployeeEntity = HrSaas.Modules.Employee.Domain.Entities.Employee;

namespace HrSaas.Modules.Employee.Application.Commands;


[Auditable(AuditAction.Create, AuditCategory.Employee, Severity = AuditSeverity.Medium)]
public sealed record CreateEmployeeCommand(
    Guid TenantId,
    string Name,
    string Department,
    string Position,
    string Email) : ICommand<Guid>;


public sealed class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.TenantId)
            .NotEmpty().WithMessage("TenantId is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Department)
            .NotEmpty().WithMessage("Department is required.")
            .MaximumLength(100).WithMessage("Department cannot exceed 100 characters.");

        RuleFor(x => x.Position)
            .NotEmpty().WithMessage("Position is required.")
            .MaximumLength(100).WithMessage("Position cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email must be a valid email address.");
    }
}


public sealed class CreateEmployeeCommandHandler(IEmployeeRepository repository)
    : IRequestHandler<CreateEmployeeCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var employee = EmployeeEntity.Create(
            command.TenantId,
            command.Name,
            command.Department,
            command.Position,
            command.Email);

        await repository.AddAsync(employee, cancellationToken).ConfigureAwait(false);
        await repository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(employee.Id);
    }
}
