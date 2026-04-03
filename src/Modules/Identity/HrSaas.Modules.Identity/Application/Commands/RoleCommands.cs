using FluentValidation;
using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Commands;

public sealed record CreateRoleCommand(
    Guid TenantId,
    string Name,
    IReadOnlyList<string> Permissions) : ICommand<Guid>;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(64);
        RuleFor(x => x.Permissions).NotEmpty();
        RuleForEach(x => x.Permissions).Must(Permission.IsValid)
            .WithMessage("'{PropertyValue}' is not a valid permission.");
    }
}

public sealed class CreateRoleCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var existing = await roleRepository
            .GetByNameAsync(request.TenantId, request.Name, cancellationToken)
            .ConfigureAwait(false);

        if (existing is not null)
            return Result<Guid>.Failure($"Role '{request.Name}' already exists.", "ROLE_EXISTS");

        var role = Role.Create(request.TenantId, request.Name, isSystemRole: false, request.Permissions);

        await roleRepository.AddAsync(role, cancellationToken).ConfigureAwait(false);
        await roleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result<Guid>.Success(role.Id);
    }
}

public sealed record UpdateRolePermissionsCommand(
    Guid TenantId,
    Guid RoleId,
    IReadOnlyList<string> Permissions) : ICommand;

public sealed class UpdateRolePermissionsCommandValidator : AbstractValidator<UpdateRolePermissionsCommand>
{
    public UpdateRolePermissionsCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.Permissions).NotEmpty();
        RuleForEach(x => x.Permissions).Must(Permission.IsValid)
            .WithMessage("'{PropertyValue}' is not a valid permission.");
    }
}

public sealed class UpdateRolePermissionsCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<UpdateRolePermissionsCommand, Result>
{
    public async Task<Result> Handle(UpdateRolePermissionsCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null || role.TenantId != request.TenantId)
            return Result.Failure("Role not found.", "NOT_FOUND");

        role.UpdatePermissions(request.Permissions);
        roleRepository.Update(role);
        await roleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

public sealed record AssignRoleCommand(
    Guid TenantId,
    Guid UserId,
    Guid NewRoleId) : ICommand;

public sealed class AssignRoleCommandValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.NewRoleId).NotEmpty();
    }
}

public sealed class AssignRoleCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IRequestHandler<AssignRoleCommand, Result>
{
    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.TenantId != request.TenantId)
            return Result.Failure("User not found.", "NOT_FOUND");

        var newRole = await roleRepository.GetByIdAsync(request.NewRoleId, cancellationToken).ConfigureAwait(false);
        if (newRole is null || newRole.TenantId != request.TenantId)
            return Result.Failure("Target role not found.", "ROLE_NOT_FOUND");

        user.AssignRole(user.RoleId, request.NewRoleId);
        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}

public sealed record DeleteRoleCommand(
    Guid TenantId,
    Guid RoleId) : ICommand;

public sealed class DeleteRoleCommandHandler(
    IRoleRepository roleRepository) : IRequestHandler<DeleteRoleCommand, Result>
{
    public async Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null || role.TenantId != request.TenantId)
            return Result.Failure("Role not found.", "NOT_FOUND");

        if (role.IsSystemRole)
            return Result.Failure("System roles cannot be deleted.", "SYSTEM_ROLE");

        roleRepository.Delete(role);
        await roleRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Result.Success();
    }
}
