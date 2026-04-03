using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using HrSaas.SharedKernel.Exceptions;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Queries;

public sealed record GetUserByIdQuery(Guid TenantId, Guid UserId) : IQuery<UserDto>;

public sealed class GetUserByIdQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, cancellationToken).ConfigureAwait(false);
        if (user is null || user.TenantId != request.TenantId)
        {
            return Result<UserDto>.Failure("User not found.", "NOT_FOUND");
        }

        return Result<UserDto>.Success(new UserDto(user.Id, user.TenantId, user.Email.Value, user.Role, user.IsActive, user.CreatedAt));
    }
}

public sealed record GetAllUsersQuery(Guid TenantId) : IQuery<IReadOnlyList<UserDto>>;

public sealed class GetAllUsersQueryHandler(
    IUserRepository userRepository) : IRequestHandler<GetAllUsersQuery, Result<IReadOnlyList<UserDto>>>
{
    public async Task<Result<IReadOnlyList<UserDto>>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await userRepository.GetAllAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var dtos = users.Select(u => new UserDto(u.Id, u.TenantId, u.Email.Value, u.Role, u.IsActive, u.CreatedAt)).ToList().AsReadOnly();
        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }
}
