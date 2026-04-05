using FluentValidation;
using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.SharedKernel.Audit;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Commands;

[Auditable(AuditAction.Register, AuditCategory.Identity, Severity = AuditSeverity.High)]
public sealed record RegisterCommand(
    Guid TenantId,
    string Email,
    string Password,
    string RoleName = "Employee") : ICommand<AuthTokenDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.RoleName).NotEmpty().MaximumLength(64);
    }
}

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return Result<AuthTokenDto>.Failure("A user with this email already exists.", "EMAIL_TAKEN");

        var role = await roleRepository.GetByNameAsync(request.TenantId, request.RoleName, cancellationToken).ConfigureAwait(false);
        if (role is null)
            return Result<AuthTokenDto>.Failure($"Role '{request.RoleName}' does not exist for this tenant.", "ROLE_NOT_FOUND");

        var email = Email.Create(request.Email);
        var hash = passwordHasher.Hash(request.Password);
        var user = AppUser.Create(request.TenantId, email, HashedPassword.FromHash(hash), role.Id);

        await userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await userRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.TenantId, user.Email.Value, role.Name, role.Permissions);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id, user.TenantId, role.Name);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            accessToken, refreshToken, DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.TenantId, user.Email.Value, role.Id, role.Name, role.Permissions, user.IsActive, user.CreatedAt)));
    }
}

[Auditable(AuditAction.Login, AuditCategory.Identity, Severity = AuditSeverity.High)]
public sealed record LoginCommand(Guid TenantId, string Email, string Password) : ICommand<AuthTokenDto>;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken).ConfigureAwait(false);
        if (user is null || !passwordHasher.Verify(request.Password, user.Password.Value))
            return Result<AuthTokenDto>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");

        if (!user.IsActive)
            return Result<AuthTokenDto>.Failure("Account is deactivated.", "ACCOUNT_DEACTIVATED");

        var role = await roleRepository.GetByIdAsync(user.RoleId, cancellationToken).ConfigureAwait(false);
        if (role is null)
            return Result<AuthTokenDto>.Failure("User role configuration is invalid.", "ROLE_NOT_FOUND");

        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id, user.TenantId, user.Email.Value, role.Name, role.Permissions);
        var refreshToken = jwtTokenService.GenerateRefreshToken(user.Id, user.TenantId, role.Name);

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            accessToken, refreshToken, DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.TenantId, user.Email.Value, role.Id, role.Name, role.Permissions, user.IsActive, user.CreatedAt)));
    }
}
