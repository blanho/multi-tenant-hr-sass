using FluentValidation;
using HrSaas.Modules.Identity.Application.Interfaces;
using HrSaas.Modules.Identity.Infrastructure.Persistence;
using HrSaas.Modules.Identity.Infrastructure.Persistence.Repositories;
using HrSaas.Modules.Identity.Infrastructure.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HrSaas.Modules.Identity;

public static class IdentityModule
{
    public static IServiceCollection AddIdentityModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<IdentityDbContext>(opts =>
            opts.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(IdentityModule).Assembly));
        services.AddValidatorsFromAssembly(typeof(IdentityModule).Assembly);

        return services;
    }
}
