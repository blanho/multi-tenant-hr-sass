using FluentValidation;
using HrSaas.Modules.Identity.Application.DTOs;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Domain.Entities;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Commands;

public sealed record RegisterCommand(
    Guid TenantId,
    string Email,
    string Password,
    string Role = "Employee") : ICommand<AuthTokenDto>;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.Role).NotEmpty().Must(r => AppUser.AllowedRoles.Contains(r))
            .WithMessage("Role must be one of: Admin, Manager, Employee");
    }
}

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<RegisterCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existing = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<AuthTokenDto>.Failure("A user with this email already exists.", "EMAIL_TAKEN");
        }

        var email = Email.Create(request.Email);
        var hash = passwordHasher.Hash(request.Password);
        var user = AppUser.Create(request.TenantId, email, HashedPassword.FromHash(hash), request.Role);

        await userRepository.AddAsync(user, cancellationToken).ConfigureAwait(false);
        await userRepository.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, user.Email.Value, user.Role);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.TenantId, user.Email.Value, user.Role, user.IsActive, user.CreatedAt)));
    }
}

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
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService) : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByEmailAsync(request.TenantId, request.Email, cancellationToken).ConfigureAwait(false);
        if (user is null || !passwordHasher.Verify(request.Password, user.Password.Value))
        {
            return Result<AuthTokenDto>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");
        }

        if (!user.IsActive)
        {
            return Result<AuthTokenDto>.Failure("Account is deactivated.", "ACCOUNT_DEACTIVATED");
        }

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.TenantId, user.Email.Value, user.Role);
        var refreshToken = jwtTokenService.GenerateRefreshToken();

        return Result<AuthTokenDto>.Success(new AuthTokenDto(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddHours(1),
            new UserDto(user.Id, user.TenantId, user.Email.Value, user.Role, user.IsActive, user.CreatedAt)));
    }
}
