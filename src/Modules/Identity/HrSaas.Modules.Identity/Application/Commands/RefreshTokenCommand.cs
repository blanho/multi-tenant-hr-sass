using FluentValidation;
using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Commands;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokenDto>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}

public sealed class RefreshTokenCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IJwtTokenService jwtTokenService) : IRequestHandler<RefreshTokenCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        (Guid userId, Guid tenantId, string role) validated;

        try
        {
            validated = jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        }
        catch
        {
            return Result<AuthTokenDto>.Failure("Invalid or expired refresh token.", "INVALID_REFRESH_TOKEN");
        }

        var user = await userRepository.GetByIdAsync(validated.userId, cancellationToken).ConfigureAwait(false);
        if (user is null || !user.IsActive)
            return Result<AuthTokenDto>.Failure("User not found or deactivated.", "USER_NOT_FOUND");

        var dbRole = await roleRepository.GetByIdAsync(user.RoleId, cancellationToken).ConfigureAwait(false);
        if (dbRole is null)
            return Result<AuthTokenDto>.Failure("User role configuration is invalid.", "ROLE_NOT_FOUND");

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.TenantId, user.Email.Value, dbRole.Name, dbRole.Permissions);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id, user.TenantId, dbRole.Name);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            accessToken, refreshToken, DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.TenantId, user.Email.Value, dbRole.Id, dbRole.Name, dbRole.Permissions, user.IsActive, user.CreatedAt)));
    }
}
