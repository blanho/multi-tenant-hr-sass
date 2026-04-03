using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Identity.Application.Commands;


public record AuthTokenDto(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    string Role,
    Guid TenantId);

public record LoginCommand(
    string Email,
    string Password,
    Guid TenantId) : ICommand<AuthTokenDto>;

public sealed class LoginCommandHandler(
    IUserRepository userRepository,
    IJwtTokenService jwtService,
    IPasswordHasher passwordHasher)
    : IRequestHandler<LoginCommand, Result<AuthTokenDto>>
{
    public async Task<Result<AuthTokenDto>> Handle(LoginCommand command, CancellationToken ct)
    {
        var user = await userRepository.GetByEmailAsync(command.Email, command.TenantId, ct);
        if (user is null || !user.IsActive)
            return Result<AuthTokenDto>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");

        if (!passwordHasher.Verify(command.Password, user.PasswordHash))
            return Result<AuthTokenDto>.Failure("Invalid credentials.", "INVALID_CREDENTIALS");

        var token = jwtService.GenerateToken(user);
        return Result<AuthTokenDto>.Success(token);
    }
}


public record RegisterCommand(
    Guid TenantId,
    string Email,
    string Password,
    string Role = "Employee") : ICommand<Guid>;

public sealed class RegisterCommandHandler(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
    : IRequestHandler<RegisterCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(RegisterCommand command, CancellationToken ct)
    {
        var existingUser = await userRepository.GetByEmailAsync(command.Email, command.TenantId, ct);
        if (existingUser is not null)
            return Result<Guid>.Failure("A user with this email already exists.", "EMAIL_EXISTS");

        var hash = passwordHasher.Hash(command.Password);
        var user = Domain.Entities.AppUser.Create(command.TenantId, command.Email, hash, command.Role);

        await userRepository.AddAsync(user, ct);
        await userRepository.SaveChangesAsync(ct);

        return Result<Guid>.Success(user.Id);
    }
}


public interface IUserRepository
{
    Task<Domain.Entities.AppUser?> GetByEmailAsync(string email, Guid tenantId, CancellationToken ct);
    Task AddAsync(Domain.Entities.AppUser user, CancellationToken ct);
    Task<int> SaveChangesAsync(CancellationToken ct);
}

public interface IJwtTokenService
{
    AuthTokenDto GenerateToken(Domain.Entities.AppUser user);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
