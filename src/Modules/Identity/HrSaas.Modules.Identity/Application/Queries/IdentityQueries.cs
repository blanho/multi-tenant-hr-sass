using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Exceptions;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Queries;

public sealed record GetUserByIdQuery(Guid TenantId, Guid UserId) : IQuery<UserDto>;

public sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.TenantId != request.TenantId)
            return Result<UserDto>.Failure("User not found.", "NOT_FOUND");

        var role = await roleRepository.GetByIdAsync(user.RoleId, cancellationToken).ConfigureAwait(false);
        var roleName = role?.Name ?? "Unknown";
        var permissions = role?.Permissions ?? [];

        return Result<UserDto>.Success(new UserDto(
            user.Id, user.TenantId, user.Email.Value, user.RoleId, roleName, permissions, user.IsActive, user.CreatedAt));
    }
}

public sealed record GetAllUsersQuery(Guid TenantId) : IQuery<IReadOnlyList<UserDto>>;

public sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository) : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var roles = await roleRepository.GetAllAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var roleLookup = roles.ToDictionary(r => r.Id);

        var dtos = users.Select(u =>
        {
            roleLookup.TryGetValue(u.RoleId, out var role);
            return new UserDto(
                u.Id, u.TenantId, u.Email.Value, u.RoleId,
                role?.Name ?? "Unknown", role?.Permissions ?? [], u.IsActive, u.CreatedAt);
        }).ToList().AsReadOnly();

        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }
}
