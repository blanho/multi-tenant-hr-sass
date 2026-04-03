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
