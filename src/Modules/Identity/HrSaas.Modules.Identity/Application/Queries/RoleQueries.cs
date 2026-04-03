using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Queries;

public sealed record GetRolesQuery(Guid TenantId) : IQuery<IReadOnlyList<RoleDto>>;

public sealed class GetRolesQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<GetRolesQuery, Result<IReadOnlyList<RoleDto>>>
{
    public async Task<Result<IReadOnlyList<RoleDto>>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = await roleRepository.GetAllAsync(request.TenantId, cancellationToken).ConfigureAwait(false);

        var dtos = roles.Select(r => new RoleDto(
            r.Id, r.TenantId, r.Name, r.IsSystemRole, r.Permissions, r.CreatedAt))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<RoleDto>>.Success(dtos);
    }
}

public sealed record GetRoleByIdQuery(Guid TenantId, Guid RoleId) : IQuery<RoleDto>;

public sealed class GetRoleByIdQueryHandler(
    IRoleRepository roleRepository) : IRequestHandler<GetRoleByIdQuery, Result<RoleDto>>
{
    public async Task<Result<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        var role = await roleRepository.GetByIdAsync(request.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null || role.TenantId != request.TenantId)
            return Result<RoleDto>.Failure("Role not found.", "NOT_FOUND");

        return Result<RoleDto>.Success(new RoleDto(
            role.Id, role.TenantId, role.Name, role.IsSystemRole, role.Permissions, role.CreatedAt));
    }
}

public sealed record GetAvailablePermissionsQuery() : IQuery<IReadOnlyList<string>>;

public sealed class GetAvailablePermissionsQueryHandler
    : IRequestHandler<GetAvailablePermissionsQuery, Result<IReadOnlyList<string>>>
{
    public Task<Result<IReadOnlyList<string>>> Handle(
        GetAvailablePermissionsQuery request,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<IReadOnlyList<string>>.Success(Permission.All));
    }
}
