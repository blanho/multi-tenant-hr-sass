#!/usr/bin/env python3
"""Generates all remaining principal-engineer-level files for HrSaas."""

import os

ROOT = "/Users/macbook/Desktop/multi-tenant-sass"

def write(rel_path: str, content: str) -> None:
    full = os.path.join(ROOT, rel_path)
    os.makedirs(os.path.dirname(full), exist_ok=True)
    with open(full, "w", encoding="utf-8") as f:
        f.write(content)
    print(f"  {rel_path}")


# ─── AppUser domain entity (rewrite) ─────────────────────────────────────────
write("src/Modules/Identity/HrSaas.Modules.Identity/Domain/Entities/AppUser.cs", """\
using HrSaas.Modules.Identity.Domain.Events;
using HrSaas.Modules.Identity.Domain.ValueObjects;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Identity.Domain.Entities;

public sealed class AppUser : BaseEntity
{
    public static readonly IReadOnlyList<string> AllowedRoles = ["Admin", "Manager", "Employee"];

    public Email Email { get; private set; } = null!;
    public HashedPassword Password { get; private set; } = null!;
    public string Role { get; private set; } = null!;

    private AppUser() { }

    public static AppUser Create(Guid tenantId, Email email, HashedPassword password, string role)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotNull(email, nameof(email));
        Guard.NotNull(password, nameof(password));
        Guard.NotNullOrWhiteSpace(role, nameof(role));

        var user = new AppUser
        {
            TenantId = tenantId,
            Email = email,
            Password = password,
            Role = role
        };

        user.AddDomainEvent(new UserRegisteredEvent(tenantId, user.Id, email.Value));
        return user;
    }

    public void ChangeRole(string newRole)
    {
        Guard.NotNullOrWhiteSpace(newRole, nameof(newRole));
        var old = Role;
        Role = newRole;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserRoleChangedEvent(TenantId, Id, old, newRole));
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new UserDeactivatedEvent(TenantId, Id));
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
""")

# ─── Tenant module ────────────────────────────────────────────────────────────
write("src/Modules/Tenant/HrSaas.Modules.Tenant/Domain/Entities/Tenant.cs", """\
using HrSaas.Modules.Tenant.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Tenant.Domain.Entities;

public enum PlanType { Free = 0, Starter = 1, Professional = 2, Enterprise = 3 }
public enum TenantStatus { Active = 0, Suspended = 1, Cancelled = 2, PendingSetup = 3 }

public sealed class Tenant : BaseEntity
{
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;
    public string ContactEmail { get; private set; } = null!;
    public PlanType Plan { get; private set; }
    public TenantStatus Status { get; private set; }
    public int MaxEmployees { get; private set; }
    public DateTime? SuspendedAt { get; private set; }

    private static readonly IReadOnlyDictionary<PlanType, int> PlanLimits = new Dictionary<PlanType, int>
    {
        [PlanType.Free] = 10,
        [PlanType.Starter] = 50,
        [PlanType.Professional] = 250,
        [PlanType.Enterprise] = int.MaxValue
    };

    private Tenant() { }

    public static Tenant Create(string name, string slug, string contactEmail, PlanType plan = PlanType.Free)
    {
        Guard.NotNullOrWhiteSpace(name, nameof(name));
        Guard.NotNullOrWhiteSpace(slug, nameof(slug));
        Guard.NotNullOrWhiteSpace(contactEmail, nameof(contactEmail));

        var tenant = new Tenant
        {
            Name = name,
            Slug = slug.ToLowerInvariant(),
            ContactEmail = contactEmail.ToLowerInvariant(),
            Plan = plan,
            Status = TenantStatus.PendingSetup,
            MaxEmployees = PlanLimits[plan]
        };

        tenant.TenantId = tenant.Id;
        tenant.AddDomainEvent(new TenantCreatedEvent(tenant.Id, name, plan.ToString()));
        return tenant;
    }

    public void Activate()
    {
        Status = TenantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Upgrade(PlanType newPlan)
    {
        if (newPlan <= Plan)
        {
            throw new InvalidOperationException("Can only upgrade to a higher plan.");
        }

        var oldPlan = Plan;
        Plan = newPlan;
        MaxEmployees = PlanLimits[newPlan];
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantPlanUpgradedEvent(TenantId, oldPlan.ToString(), newPlan.ToString()));
    }

    public void Suspend(string reason)
    {
        Status = TenantStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new TenantSuspendedEvent(TenantId, reason));
    }

    public void Reinstate()
    {
        Status = TenantStatus.Active;
        SuspendedAt = null;
        UpdatedAt = DateTime.UtcNow;
    }
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Domain/Events/TenantDomainEvents.cs", """\
using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Tenant.Domain.Events;

public sealed record TenantCreatedEvent(Guid TenantId, string Name, string Plan) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantPlanUpgradedEvent(Guid TenantId, string OldPlan, string NewPlan) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record TenantSuspendedEvent(Guid TenantId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Application/DTOs/TenantDtos.cs", """\
namespace HrSaas.Modules.Tenant.Application.DTOs;

public sealed record TenantDto(
    Guid Id,
    string Name,
    string Slug,
    string ContactEmail,
    string Plan,
    string Status,
    int MaxEmployees,
    DateTime CreatedAt);
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Application/Interfaces/ITenantRepository.cs", """\
using HrSaas.Modules.Tenant.Domain.Entities;

namespace HrSaas.Modules.Tenant.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Tenant tenant, CancellationToken ct = default);
    void Update(Tenant tenant);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Application/Commands/TenantCommands.cs", """\
using FluentValidation;
using HrSaas.Modules.Tenant.Application.DTOs;
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Tenant.Application.Commands;

public sealed record CreateTenantCommand(string Name, string Slug, string ContactEmail, PlanType Plan = PlanType.Free) : ICommand<Guid>;

public sealed class CreateTenantCommandValidator : AbstractValidator<CreateTenantCommand>
{
    public CreateTenantCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Slug).NotEmpty().MaximumLength(100).Matches("^[a-z0-9-]+$").WithMessage("Slug must be lowercase alphanumeric with hyphens.");
        RuleFor(x => x.ContactEmail).NotEmpty().EmailAddress().MaximumLength(254);
    }
}

public sealed class CreateTenantCommandHandler(ITenantRepository repo) : IRequestHandler<CreateTenantCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateTenantCommand request, CancellationToken cancellationToken)
    {
        var existing = await repo.GetBySlugAsync(request.Slug, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<Guid>.Failure("A tenant with this slug already exists.", "SLUG_TAKEN");
        }

        var tenant = Tenant.Create(request.Name, request.Slug, request.ContactEmail, request.Plan);
        tenant.Activate();
        await repo.AddAsync(tenant, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(tenant.Id);
    }
}

public sealed record SuspendTenantCommand(Guid TenantId, string Reason) : ICommand;

public sealed class SuspendTenantCommandHandler(ITenantRepository repo) : IRequestHandler<SuspendTenantCommand, Result>
{
    public async Task<Result> Handle(SuspendTenantCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure("Tenant not found.", "NOT_FOUND");
        }

        tenant.Suspend(request.Reason);
        repo.Update(tenant);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record UpgradePlanCommand(Guid TenantId, PlanType NewPlan) : ICommand;

public sealed class UpgradePlanCommandHandler(ITenantRepository repo) : IRequestHandler<UpgradePlanCommand, Result>
{
    public async Task<Result> Handle(UpgradePlanCommand request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result.Failure("Tenant not found.", "NOT_FOUND");
        }

        tenant.Upgrade(request.NewPlan);
        repo.Update(tenant);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Application/Queries/TenantQueries.cs", """\
using HrSaas.Modules.Tenant.Application.DTOs;
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Tenant.Application.Queries;

public sealed record GetTenantByIdQuery(Guid TenantId) : IQuery<TenantDto>;

public sealed class GetTenantByIdQueryHandler(ITenantRepository repo) : IRequestHandler<GetTenantByIdQuery, Result<TenantDto>>
{
    public async Task<Result<TenantDto>> Handle(GetTenantByIdQuery request, CancellationToken cancellationToken)
    {
        var tenant = await repo.GetByIdAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (tenant is null)
        {
            return Result<TenantDto>.Failure("Tenant not found.", "NOT_FOUND");
        }

        return Result<TenantDto>.Success(new TenantDto(
            tenant.Id, tenant.Name, tenant.Slug, tenant.ContactEmail,
            tenant.Plan.ToString(), tenant.Status.ToString(), tenant.MaxEmployees, tenant.CreatedAt));
    }
}

public sealed record GetAllTenantsQuery : IQuery<IReadOnlyList<TenantDto>>;

public sealed class GetAllTenantsQueryHandler(ITenantRepository repo) : IRequestHandler<GetAllTenantsQuery, Result<IReadOnlyList<TenantDto>>>
{
    public async Task<Result<IReadOnlyList<TenantDto>>> Handle(GetAllTenantsQuery request, CancellationToken cancellationToken)
    {
        var tenants = await repo.GetAllAsync(cancellationToken).ConfigureAwait(false);
        var dtos = tenants.Select(t => new TenantDto(
            t.Id, t.Name, t.Slug, t.ContactEmail,
            t.Plan.ToString(), t.Status.ToString(), t.MaxEmployees, t.CreatedAt)).ToList().AsReadOnly();
        return Result<IReadOnlyList<TenantDto>>.Success(dtos);
    }
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/TenantDbContext.cs", """\
using HrSaas.Modules.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence;

public sealed class TenantDbContext(DbContextOptions<TenantDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("tenant");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TenantDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/Configurations/TenantConfiguration.cs", """\
using HrSaas.Modules.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence.Configurations;

public sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();
        builder.Property(t => t.Name).HasMaxLength(200).IsRequired();
        builder.Property(t => t.Slug).HasMaxLength(100).IsRequired();
        builder.Property(t => t.ContactEmail).HasMaxLength(254).IsRequired();
        builder.Property(t => t.Plan).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(t => t.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(t => t.MaxEmployees).IsRequired();
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
    }
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/Repositories/TenantRepository.cs", """\
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Tenant.Infrastructure.Persistence.Repositories;

public sealed class TenantRepository(TenantDbContext dbContext) : ITenantRepository
{
    public async Task<Tenant?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(t => t.Id == id && !t.IsDeleted, ct).ConfigureAwait(false);

    public async Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
        await dbContext.Tenants.FirstOrDefaultAsync(t => t.Slug == slug && !t.IsDeleted, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default) =>
        await dbContext.Tenants.Where(t => !t.IsDeleted).ToListAsync(ct).ConfigureAwait(false);

    public async Task AddAsync(Tenant tenant, CancellationToken ct = default) =>
        await dbContext.Tenants.AddAsync(tenant, ct).ConfigureAwait(false);

    public void Update(Tenant tenant) => dbContext.Tenants.Update(tenant);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
""")

write("src/Modules/Tenant/HrSaas.Modules.Tenant/TenantModule.cs", """\
using FluentValidation;
using HrSaas.Modules.Tenant.Application.Interfaces;
using HrSaas.Modules.Tenant.Infrastructure.Persistence;
using HrSaas.Modules.Tenant.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Tenant;

public static class TenantModule
{
    public static IServiceCollection AddTenantModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<TenantDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TenantModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(TenantModule).Assembly);

        return services;
    }
}
""")

print("Tenant module done")

# ─── Leave module ─────────────────────────────────────────────────────────────
write("src/Modules/Leave/HrSaas.Modules.Leave/Domain/Entities/LeaveRequest.cs", """\
using HrSaas.Modules.Leave.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;
using HrSaas.SharedKernel.Guards;

namespace HrSaas.Modules.Leave.Domain.Entities;

public enum LeaveType { Annual = 0, Sick = 1, Maternity = 2, Paternity = 3, Unpaid = 4, Emergency = 5 }
public enum LeaveStatus { Pending = 0, Approved = 1, Rejected = 2, Cancelled = 3 }

public sealed class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; private set; }
    public LeaveType Type { get; private set; }
    public LeaveStatus Status { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Reason { get; private set; } = null!;
    public string? RejectionNote { get; private set; }
    public Guid? ApprovedByUserId { get; private set; }
    public DateTime? DecisionAt { get; private set; }

    private LeaveRequest() { }

    public static LeaveRequest Apply(Guid tenantId, Guid employeeId, LeaveType type, DateTime start, DateTime end, string reason)
    {
        Guard.NotEmpty(tenantId, nameof(tenantId));
        Guard.NotEmpty(employeeId, nameof(employeeId));
        Guard.NotNullOrWhiteSpace(reason, nameof(reason));

        if (end <= start)
        {
            throw new DomainException("End date must be after start date.");
        }

        var request = new LeaveRequest
        {
            TenantId = tenantId,
            EmployeeId = employeeId,
            Type = type,
            Status = LeaveStatus.Pending,
            StartDate = start,
            EndDate = end,
            Reason = reason
        };

        request.AddDomainEvent(new LeaveAppliedEvent(tenantId, request.Id, employeeId, type.ToString()));
        return request;
    }

    public void Approve(Guid approvedByUserId)
    {
        if (Status != LeaveStatus.Pending)
        {
            throw new DomainException($"Cannot approve a leave request in {Status} status.");
        }

        Status = LeaveStatus.Approved;
        ApprovedByUserId = approvedByUserId;
        DecisionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LeaveApprovedEvent(TenantId, Id, EmployeeId, approvedByUserId));
    }

    public void Reject(Guid rejectedByUserId, string note)
    {
        if (Status != LeaveStatus.Pending)
        {
            throw new DomainException($"Cannot reject a leave request in {Status} status.");
        }

        Status = LeaveStatus.Rejected;
        RejectionNote = note;
        DecisionAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new LeaveRejectedEvent(TenantId, Id, EmployeeId, note));
    }

    public void Cancel()
    {
        if (Status == LeaveStatus.Approved || Status == LeaveStatus.Rejected)
        {
            throw new DomainException("Cannot cancel an already decided leave request.");
        }

        Status = LeaveStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public int GetDurationDays() => (int)(EndDate - StartDate).TotalDays + 1;
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Domain/Events/LeaveDomainEvents.cs", """\
using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Leave.Domain.Events;

public sealed record LeaveAppliedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, string LeaveType) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveApprovedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, Guid ApprovedBy) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record LeaveRejectedEvent(Guid TenantId, Guid LeaveRequestId, Guid EmployeeId, string Note) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Application/DTOs/LeaveDtos.cs", """\
namespace HrSaas.Modules.Leave.Application.DTOs;

public sealed record LeaveRequestDto(
    Guid Id,
    Guid TenantId,
    Guid EmployeeId,
    string Type,
    string Status,
    DateTime StartDate,
    DateTime EndDate,
    string Reason,
    string? RejectionNote,
    int DurationDays,
    DateTime CreatedAt);
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Application/Interfaces/ILeaveRepository.cs", """\
using HrSaas.Modules.Leave.Domain.Entities;

namespace HrSaas.Modules.Leave.Application.Interfaces;

public interface ILeaveRepository
{
    Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default);
    Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(LeaveRequest request, CancellationToken ct = default);
    void Update(LeaveRequest request);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Application/Commands/LeaveCommands.cs", """\
using FluentValidation;
using HrSaas.Modules.Leave.Application.DTOs;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Leave.Application.Commands;

public sealed record ApplyLeaveCommand(
    Guid TenantId,
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string Reason) : ICommand<Guid>;

public sealed class ApplyLeaveCommandValidator : AbstractValidator<ApplyLeaveCommand>
{
    public ApplyLeaveCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.EmployeeId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.StartDate).LessThan(x => x.EndDate).WithMessage("Start date must be before end date.");
        RuleFor(x => x.StartDate).GreaterThanOrEqualTo(DateTime.UtcNow.Date).WithMessage("Cannot apply leave in the past.");
    }
}

public sealed class ApplyLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<ApplyLeaveCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(ApplyLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = LeaveRequest.Apply(request.TenantId, request.EmployeeId, request.Type, request.StartDate, request.EndDate, request.Reason);
        await repo.AddAsync(leave, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(leave.Id);
    }
}

public sealed record ApproveLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid ApprovedByUserId) : ICommand;

public sealed class ApproveLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<ApproveLeaveCommand, Result>
{
    public async Task<Result> Handle(ApproveLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdAsync(request.LeaveRequestId, cancellationToken).ConfigureAwait(false);
        if (leave is null || leave.TenantId != request.TenantId)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        leave.Approve(request.ApprovedByUserId);
        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record RejectLeaveCommand(Guid TenantId, Guid LeaveRequestId, Guid RejectedByUserId, string Note) : ICommand;

public sealed class RejectLeaveCommandHandler(ILeaveRepository repo) : IRequestHandler<RejectLeaveCommand, Result>
{
    public async Task<Result> Handle(RejectLeaveCommand request, CancellationToken cancellationToken)
    {
        var leave = await repo.GetByIdAsync(request.LeaveRequestId, cancellationToken).ConfigureAwait(false);
        if (leave is null || leave.TenantId != request.TenantId)
        {
            return Result.Failure("Leave request not found.", "NOT_FOUND");
        }

        leave.Reject(request.RejectedByUserId, request.Note);
        repo.Update(leave);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Application/Queries/LeaveQueries.cs", """\
using HrSaas.Modules.Leave.Application.DTOs;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Leave.Application.Queries;

public sealed record GetLeavesByEmployeeQuery(Guid TenantId, Guid EmployeeId) : IQuery<IReadOnlyList<LeaveRequestDto>>;

public sealed class GetLeavesByEmployeeQueryHandler(ILeaveRepository repo) : IRequestHandler<GetLeavesByEmployeeQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeavesByEmployeeQuery request, CancellationToken cancellationToken)
    {
        var leaves = await repo.GetByEmployeeAsync(request.TenantId, request.EmployeeId, cancellationToken).ConfigureAwait(false);
        var dtos = leaves.Select(Map).ToList().AsReadOnly();
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }

    private static LeaveRequestDto Map(Domain.Entities.LeaveRequest l) =>
        new(l.Id, l.TenantId, l.EmployeeId, l.Type.ToString(), l.Status.ToString(),
            l.StartDate, l.EndDate, l.Reason, l.RejectionNote, l.GetDurationDays(), l.CreatedAt);
}

public sealed record GetPendingLeavesQuery(Guid TenantId) : IQuery<IReadOnlyList<LeaveRequestDto>>;

public sealed class GetPendingLeavesQueryHandler(ILeaveRepository repo) : IRequestHandler<GetPendingLeavesQuery, Result<IReadOnlyList<LeaveRequestDto>>>
{
    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetPendingLeavesQuery request, CancellationToken cancellationToken)
    {
        var leaves = await repo.GetPendingAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        var dtos = leaves.Select(l =>
            new LeaveRequestDto(l.Id, l.TenantId, l.EmployeeId, l.Type.ToString(), l.Status.ToString(),
                l.StartDate, l.EndDate, l.Reason, l.RejectionNote, l.GetDurationDays(), l.CreatedAt))
            .ToList().AsReadOnly();
        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Infrastructure/Persistence/LeaveDbContext.cs", """\
using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence;

public sealed class LeaveDbContext(DbContextOptions<LeaveDbContext> options) : DbContext(options)
{
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("leave");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LeaveDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Infrastructure/Persistence/Configurations/LeaveRequestConfiguration.cs", """\
using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("leave_requests");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Id).ValueGeneratedNever();
        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.EmployeeId).IsRequired();
        builder.Property(l => l.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(l => l.StartDate).IsRequired();
        builder.Property(l => l.EndDate).IsRequired();
        builder.Property(l => l.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(l => l.RejectionNote).HasMaxLength(1000);
        builder.HasQueryFilter(l => l.TenantId == l.TenantId && !l.IsDeleted);
        builder.HasIndex(l => new { l.TenantId, l.EmployeeId });
        builder.HasIndex(l => new { l.TenantId, l.Status });
    }
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/Infrastructure/Persistence/Repositories/LeaveRepository.cs", """\
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Leave.Infrastructure.Persistence.Repositories;

public sealed class LeaveRepository(LeaveDbContext dbContext) : ILeaveRepository
{
    public async Task<LeaveRequest?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.FirstOrDefaultAsync(l => l.Id == id, ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LeaveRequest>> GetByEmployeeAsync(Guid tenantId, Guid employeeId, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.Where(l => l.TenantId == tenantId && l.EmployeeId == employeeId && !l.IsDeleted).OrderByDescending(l => l.CreatedAt).ToListAsync(ct).ConfigureAwait(false);

    public async Task<IReadOnlyList<LeaveRequest>> GetPendingAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.Where(l => l.TenantId == tenantId && l.Status == LeaveStatus.Pending && !l.IsDeleted).OrderBy(l => l.StartDate).ToListAsync(ct).ConfigureAwait(false);

    public async Task AddAsync(LeaveRequest request, CancellationToken ct = default) =>
        await dbContext.LeaveRequests.AddAsync(request, ct).ConfigureAwait(false);

    public void Update(LeaveRequest request) => dbContext.LeaveRequests.Update(request);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
""")

write("src/Modules/Leave/HrSaas.Modules.Leave/LeaveModule.cs", """\
using FluentValidation;
using HrSaas.Modules.Leave.Application.Interfaces;
using HrSaas.Modules.Leave.Infrastructure.Persistence;
using HrSaas.Modules.Leave.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Leave;

public static class LeaveModule
{
    public static IServiceCollection AddLeaveModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<LeaveDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ILeaveRepository, LeaveRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(LeaveModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(LeaveModule).Assembly);

        return services;
    }
}
""")

print("Leave module done")

# ─── Billing module ───────────────────────────────────────────────────────────
write("src/Modules/Billing/HrSaas.Modules.Billing/Domain/Entities/Subscription.cs", """\
using HrSaas.Modules.Billing.Domain.Events;
using HrSaas.SharedKernel.Entities;
using HrSaas.SharedKernel.Exceptions;

namespace HrSaas.Modules.Billing.Domain.Entities;

public enum SubscriptionStatus { Trial = 0, Active = 1, PastDue = 2, Cancelled = 3, Expired = 4 }
public enum BillingCycle { Monthly = 0, Annual = 1 }

public sealed class Subscription : BaseEntity
{
    public string PlanName { get; private set; } = null!;
    public SubscriptionStatus Status { get; private set; }
    public BillingCycle Cycle { get; private set; }
    public decimal PricePerCycle { get; private set; }
    public int MaxSeats { get; private set; }
    public int UsedSeats { get; private set; }
    public DateTime? TrialEndsAt { get; private set; }
    public DateTime? CurrentPeriodStart { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }
    public string? ExternalSubscriptionId { get; private set; }

    private Subscription() { }

    public static Subscription CreateFree(Guid tenantId)
    {
        var sub = new Subscription
        {
            TenantId = tenantId,
            PlanName = "Free",
            Status = SubscriptionStatus.Active,
            Cycle = BillingCycle.Monthly,
            PricePerCycle = 0,
            MaxSeats = 10,
            CurrentPeriodStart = DateTime.UtcNow
        };
        sub.AddDomainEvent(new SubscriptionCreatedEvent(tenantId, sub.Id, "Free"));
        return sub;
    }

    public static Subscription CreateTrial(Guid tenantId, string planName, int trialDays = 14)
    {
        var sub = new Subscription
        {
            TenantId = tenantId,
            PlanName = planName,
            Status = SubscriptionStatus.Trial,
            Cycle = BillingCycle.Monthly,
            PricePerCycle = 0,
            MaxSeats = 25,
            TrialEndsAt = DateTime.UtcNow.AddDays(trialDays)
        };
        sub.AddDomainEvent(new SubscriptionCreatedEvent(tenantId, sub.Id, planName));
        return sub;
    }

    public void Activate(decimal price, BillingCycle cycle, string? externalId = null)
    {
        Status = SubscriptionStatus.Active;
        PricePerCycle = price;
        Cycle = cycle;
        ExternalSubscriptionId = externalId;
        CurrentPeriodStart = DateTime.UtcNow;
        CurrentPeriodEnd = cycle == BillingCycle.Monthly
            ? DateTime.UtcNow.AddMonths(1)
            : DateTime.UtcNow.AddYears(1);
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionActivatedEvent(TenantId, Id, PlanName));
    }

    public void Cancel(string reason)
    {
        if (Status == SubscriptionStatus.Cancelled)
        {
            throw new DomainException("Subscription is already cancelled.");
        }

        Status = SubscriptionStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
        AddDomainEvent(new SubscriptionCancelledEvent(TenantId, Id, reason));
    }

    public void MarkPastDue() { Status = SubscriptionStatus.PastDue; UpdatedAt = DateTime.UtcNow; }

    public bool CanAddSeat() => UsedSeats < MaxSeats;

    public void IncrementSeats()
    {
        if (!CanAddSeat())
        {
            throw new DomainException($"Seat limit of {MaxSeats} reached for plan {PlanName}.");
        }

        UsedSeats++;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DecrementSeats() { if (UsedSeats > 0) { UsedSeats--; UpdatedAt = DateTime.UtcNow; } }
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Domain/Events/BillingDomainEvents.cs", """\
using HrSaas.SharedKernel.Events;

namespace HrSaas.Modules.Billing.Domain.Events;

public sealed record SubscriptionCreatedEvent(Guid TenantId, Guid SubscriptionId, string PlanName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionActivatedEvent(Guid TenantId, Guid SubscriptionId, string PlanName) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}

public sealed record SubscriptionCancelledEvent(Guid TenantId, Guid SubscriptionId, string Reason) : IDomainEvent
{
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Application/DTOs/BillingDtos.cs", """\
namespace HrSaas.Modules.Billing.Application.DTOs;

public sealed record SubscriptionDto(
    Guid Id,
    Guid TenantId,
    string PlanName,
    string Status,
    string BillingCycle,
    decimal PricePerCycle,
    int MaxSeats,
    int UsedSeats,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    DateTime CreatedAt);
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Application/Interfaces/ISubscriptionRepository.cs", """\
using HrSaas.Modules.Billing.Domain.Entities;

namespace HrSaas.Modules.Billing.Application.Interfaces;

public interface ISubscriptionRepository
{
    Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Subscription?> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default);
    Task AddAsync(Subscription subscription, CancellationToken ct = default);
    void Update(Subscription subscription);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Application/Commands/BillingCommands.cs", """\
using FluentValidation;
using HrSaas.Modules.Billing.Application.DTOs;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Domain.Entities;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Billing.Application.Commands;

public sealed record CreateFreeSubscriptionCommand(Guid TenantId) : ICommand<Guid>;

public sealed class CreateFreeSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<CreateFreeSubscriptionCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateFreeSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var existing = await repo.GetActiveByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
        {
            return Result<Guid>.Failure("Tenant already has an active subscription.", "ALREADY_SUBSCRIBED");
        }

        var subscription = Subscription.CreateFree(request.TenantId);
        await repo.AddAsync(subscription, cancellationToken).ConfigureAwait(false);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result<Guid>.Success(subscription.Id);
    }
}

public sealed record ActivateSubscriptionCommand(Guid TenantId, Guid SubscriptionId, decimal Price, BillingCycle Cycle, string? ExternalId) : ICommand;

public sealed class ActivateSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<ActivateSubscriptionCommand, Result>
{
    public async Task<Result> Handle(ActivateSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetByIdAsync(request.SubscriptionId, cancellationToken).ConfigureAwait(false);
        if (sub is null || sub.TenantId != request.TenantId)
        {
            return Result.Failure("Subscription not found.", "NOT_FOUND");
        }

        sub.Activate(request.Price, request.Cycle, request.ExternalId);
        repo.Update(sub);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}

public sealed record CancelSubscriptionCommand(Guid TenantId, Guid SubscriptionId, string Reason) : ICommand;

public sealed class CancelSubscriptionCommandValidator : AbstractValidator<CancelSubscriptionCommand>
{
    public CancelSubscriptionCommandValidator()
    {
        RuleFor(x => x.TenantId).NotEmpty();
        RuleFor(x => x.SubscriptionId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(500);
    }
}

public sealed class CancelSubscriptionCommandHandler(ISubscriptionRepository repo) : IRequestHandler<CancelSubscriptionCommand, Result>
{
    public async Task<Result> Handle(CancelSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetByIdAsync(request.SubscriptionId, cancellationToken).ConfigureAwait(false);
        if (sub is null || sub.TenantId != request.TenantId)
        {
            return Result.Failure("Subscription not found.", "NOT_FOUND");
        }

        sub.Cancel(request.Reason);
        repo.Update(sub);
        await repo.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Result.Success();
    }
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Application/Queries/BillingQueries.cs", """\
using HrSaas.Modules.Billing.Application.DTOs;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.SharedKernel.CQRS;
using MediatR;

namespace HrSaas.Modules.Billing.Application.Queries;

public sealed record GetSubscriptionByTenantQuery(Guid TenantId) : IQuery<SubscriptionDto>;

public sealed class GetSubscriptionByTenantQueryHandler(ISubscriptionRepository repo) : IRequestHandler<GetSubscriptionByTenantQuery, Result<SubscriptionDto>>
{
    public async Task<Result<SubscriptionDto>> Handle(GetSubscriptionByTenantQuery request, CancellationToken cancellationToken)
    {
        var sub = await repo.GetActiveByTenantAsync(request.TenantId, cancellationToken).ConfigureAwait(false);
        if (sub is null)
        {
            return Result<SubscriptionDto>.Failure("No active subscription found.", "NOT_FOUND");
        }

        return Result<SubscriptionDto>.Success(new SubscriptionDto(
            sub.Id, sub.TenantId, sub.PlanName, sub.Status.ToString(), sub.Cycle.ToString(),
            sub.PricePerCycle, sub.MaxSeats, sub.UsedSeats, sub.TrialEndsAt, sub.CurrentPeriodEnd, sub.CreatedAt));
    }
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Infrastructure/Persistence/BillingDbContext.cs", """\
using HrSaas.Modules.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence;

public sealed class BillingDbContext(DbContextOptions<BillingDbContext> options) : DbContext(options)
{
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("billing");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BillingDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Infrastructure/Persistence/Configurations/SubscriptionConfiguration.cs", """\
using HrSaas.Modules.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence.Configurations;

public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).ValueGeneratedNever();
        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.PlanName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(s => s.Cycle).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(s => s.PricePerCycle).HasPrecision(18, 4).IsRequired();
        builder.Property(s => s.MaxSeats).IsRequired();
        builder.Property(s => s.UsedSeats).IsRequired();
        builder.Property(s => s.ExternalSubscriptionId).HasMaxLength(200);
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.HasIndex(s => s.TenantId);
    }
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/Infrastructure/Persistence/Repositories/SubscriptionRepository.cs", """\
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HrSaas.Modules.Billing.Infrastructure.Persistence.Repositories;

public sealed class SubscriptionRepository(BillingDbContext dbContext) : ISubscriptionRepository
{
    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, ct).ConfigureAwait(false);

    public async Task<Subscription?> GetActiveByTenantAsync(Guid tenantId, CancellationToken ct = default) =>
        await dbContext.Subscriptions
            .Where(s => s.TenantId == tenantId && s.Status != SubscriptionStatus.Cancelled && !s.IsDeleted)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync(ct)
            .ConfigureAwait(false);

    public async Task AddAsync(Subscription subscription, CancellationToken ct = default) =>
        await dbContext.Subscriptions.AddAsync(subscription, ct).ConfigureAwait(false);

    public void Update(Subscription subscription) => dbContext.Subscriptions.Update(subscription);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
}
""")

write("src/Modules/Billing/HrSaas.Modules.Billing/BillingModule.cs", """\
using FluentValidation;
using HrSaas.Modules.Billing.Application.Interfaces;
using HrSaas.Modules.Billing.Infrastructure.Persistence;
using HrSaas.Modules.Billing.Infrastructure.Persistence.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Billing;

public static class BillingModule
{
    public static IServiceCollection AddBillingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BillingDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(BillingModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(BillingModule).Assembly);

        return services;
    }
}
""")

print("Billing module done")
print("All modules generated successfully.")
