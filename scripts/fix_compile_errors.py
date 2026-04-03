import os

base = "/Users/macbook/Desktop/multi-tenant-sass"

files = {}

# 1. Fix Employee.cs -- remove duplicate namespace/class (only keep first 109 lines of good content)
employee_entity_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Domain/Entities/Employee.cs"
with open(employee_entity_path, 'r') as f:
    lines = f.readlines()

# Find the first closing brace that ends the class (after ValidateEmail ends)
# The clean content ends at the line with "}" that closes the class
# Look for the second "namespace" declaration and truncate before it
clean_lines = []
for i, line in enumerate(lines):
    if line.strip() == "namespace HrSaas.Modules.Employee.Domain.Entities;" and i > 5:
        break
    clean_lines.append(line)

# Remove any trailing empty lines then add one newline
while clean_lines and clean_lines[-1].strip() == "":
    clean_lines.pop()
clean_lines.append("\n")

with open(employee_entity_path, 'w') as f:
    f.writelines(clean_lines)
print(f"OK: Employee.cs truncated to {len(clean_lines)} lines")

# 2. Fix IEmployeeRepository.cs -- change Entities.Employee to Employee
repo_interface_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Application/Interfaces/IEmployeeRepository.cs"
content = (
    "using HrSaas.Modules.Employee.Domain.Entities;\n"
    "\n"
    "namespace HrSaas.Modules.Employee.Application.Interfaces;\n"
    "\n"
    "public interface IEmployeeRepository\n"
    "{\n"
    "    Task<Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);\n"
    "\n"
    "    Task<IReadOnlyList<Employee>> GetAllAsync(CancellationToken ct = default);\n"
    "\n"
    "    Task<IReadOnlyList<Employee>> GetByDepartmentAsync(string department, CancellationToken ct = default);\n"
    "\n"
    "    Task AddAsync(Employee employee, CancellationToken ct = default);\n"
    "\n"
    "    void Update(Employee employee);\n"
    "\n"
    "    void Delete(Employee employee);\n"
    "\n"
    "    Task<int> SaveChangesAsync(CancellationToken ct = default);\n"
    "}\n"
)
with open(repo_interface_path, 'w') as f:
    f.write(content)
print("OK: IEmployeeRepository.cs")

# 3. Move IEmployeeDbContext to Application/Interfaces and fix its namespace
idb_app_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Application/Interfaces/IEmployeeDbContext.cs"
idb_content = (
    "using HrSaas.Modules.Employee.Domain.Entities;\n"
    "using Microsoft.EntityFrameworkCore;\n"
    "\n"
    "namespace HrSaas.Modules.Employee.Application.Interfaces;\n"
    "\n"
    "public interface IEmployeeDbContext\n"
    "{\n"
    "    DbSet<Employee> Employees { get; }\n"
    "    Task<int> SaveChangesAsync(CancellationToken ct = default);\n"
    "}\n"
)
with open(idb_app_path, 'w') as f:
    f.write(idb_content)
print("OK: IEmployeeDbContext.cs moved to Application/Interfaces")

# 4. Remove the old IEmployeeDbContext from Infrastructure (replace with redirect)
idb_infra_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Infrastructure/Persistence/IEmployeeDbContext.cs"
os.remove(idb_infra_path)
print("OK: Removed old IEmployeeDbContext.cs from Infrastructure")

# 5. Fix EmployeeRepository.cs -- implement Delete, fix type references
repo_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Infrastructure/Persistence/Repositories/EmployeeRepository.cs"
with open(repo_path, 'r') as f:
    repo_content = f.read()

# Check if it has a Delete method
if "void Delete(Employee" not in repo_content and "void Delete(Domain.Entities.Employee" not in repo_content:
    # Add Delete method before SaveChangesAsync
    repo_content = repo_content.replace(
        "    public Task<int> SaveChangesAsync(CancellationToken ct = default)",
        "    public void Delete(Employee employee)\n"
        "    {\n"
        "        _dbContext.Employees.Remove(employee);\n"
        "    }\n"
        "\n"
        "    public Task<int> SaveChangesAsync(CancellationToken ct = default)"
    )
with open(repo_path, 'w') as f:
    f.write(repo_content)
print("OK: EmployeeRepository.cs Delete method added")

# 6. Fix EmployeeQueries.cs -- fix using for IEmployeeDbContext
queries_path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Application/Queries/EmployeeQueries.cs"
with open(queries_path, 'r') as f:
    queries = f.read()

# The file already has: using HrSaas.Modules.Employee.Application.Interfaces;
# which is where IEmployeeDbContext now lives -- no change needed if using is present
if "using HrSaas.Modules.Employee.Application.Interfaces;" not in queries:
    queries = queries.replace(
        "using HrSaas.SharedKernel.CQRS;",
        "using HrSaas.Modules.Employee.Application.Interfaces;\nusing HrSaas.SharedKernel.CQRS;"
    )
    with open(queries_path, 'w') as f:
        f.write(queries)
    print("OK: EmployeeQueries.cs using added")
else:
    print("OK: EmployeeQueries.cs using already present")

# 7. Fix Tenant module -- add type alias for Tenant entity in conflicting files
tenant_files_needing_alias = [
    f"{base}/src/Modules/Tenant/HrSaas.Modules.Tenant/Application/Interfaces/ITenantRepository.cs",
    f"{base}/src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/Configurations/TenantConfiguration.cs",
    f"{base}/src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/Repositories/TenantRepository.cs",
    f"{base}/src/Modules/Tenant/HrSaas.Modules.Tenant/Infrastructure/Persistence/TenantDbContext.cs",
]

for fpath in tenant_files_needing_alias:
    if not os.path.exists(fpath):
        print(f"SKIP (not found): {os.path.basename(fpath)}")
        continue
    with open(fpath, 'r') as f:
        content = f.read()
    old_using = "using HrSaas.Modules.Tenant.Domain.Entities;"
    new_using = "using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;"
    if old_using in content and new_using not in content:
        content = content.replace(old_using, new_using)
        # Replace bare Tenant references with TenantEntity (but not in namespace names)
        import re
        # Replace 'Tenant?' with 'TenantEntity?'
        content = re.sub(r'\bTenant\?', 'TenantEntity?', content)
        # Replace 'Tenant>' with 'TenantEntity>' (e.g. IReadOnlyList<Tenant>)
        content = re.sub(r'\bTenant>', 'TenantEntity>', content)
        # Replace 'Tenant ' (type declaration before variable name) with 'TenantEntity '
        content = re.sub(r'\bTenant (tenant\b)', r'TenantEntity \1', content)
        # Replace 'Tenant>' in generics
        content = re.sub(r'<Tenant>', '<TenantEntity>', content)
        # Replace AddAsync(Tenant etc
        content = re.sub(r'\(Tenant\b', '(TenantEntity', content)
        # Replace IEntityTypeConfiguration<Tenant>
        content = re.sub(r'IEntityTypeConfiguration<Tenant>', 'IEntityTypeConfiguration<TenantEntity>', content)
        # Replace EntityTypeBuilder<Tenant>
        content = re.sub(r'EntityTypeBuilder<Tenant>', 'EntityTypeBuilder<TenantEntity>', content)
        # Replace DbSet<Tenant>
        content = re.sub(r'DbSet<Tenant>', 'DbSet<TenantEntity>', content)
        with open(fpath, 'w') as f:
            f.write(content)
        print(f"OK: {os.path.basename(fpath)} - Tenant alias applied")
    elif new_using in content:
        print(f"OK: {os.path.basename(fpath)} - alias already present")
    else:
        print(f"SKIP: {os.path.basename(fpath)} - no Tenant entity using found")

# 8. Fix JwtTokenService.cs -- rewrite to match IJwtTokenService interface
jwt_path = f"{base}/src/Modules/Identity/HrSaas.Modules.Identity/Infrastructure/Services/JwtTokenService.cs"
jwt_content = (
    "using HrSaas.Modules.Identity.Application.Interfaces;\n"
    "using Microsoft.Extensions.Options;\n"
    "using Microsoft.IdentityModel.Tokens;\n"
    "using System.IdentityModel.Tokens.Jwt;\n"
    "using System.Security.Claims;\n"
    "using System.Text;\n"
    "\n"
    "namespace HrSaas.Modules.Identity.Infrastructure.Services;\n"
    "\n"
    "public sealed class JwtOptions\n"
    "{\n"
    "    public required string SecretKey { get; init; }\n"
    "    public required string Issuer { get; init; }\n"
    "    public required string Audience { get; init; }\n"
    "    public int ExpiryMinutes { get; init; } = 60;\n"
    "}\n"
    "\n"
    "public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService\n"
    "{\n"
    "    private readonly JwtOptions _opts = options.Value;\n"
    "\n"
    "    public string GenerateAccessToken(Guid userId, Guid tenantId, string email, string role)\n"
    "    {\n"
    "        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.SecretKey));\n"
    "        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);\n"
    "\n"
    "        var claims = new[]\n"
    "        {\n"
    "            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),\n"
    "            new Claim(JwtRegisteredClaimNames.Email, email),\n"
    "            new Claim(\"tenant_id\", tenantId.ToString()),\n"
    "            new Claim(ClaimTypes.Role, role),\n"
    "            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),\n"
    "        };\n"
    "\n"
    "        var token = new JwtSecurityToken(\n"
    "            issuer: _opts.Issuer,\n"
    "            audience: _opts.Audience,\n"
    "            claims: claims,\n"
    "            expires: DateTime.UtcNow.AddMinutes(_opts.ExpiryMinutes),\n"
    "            signingCredentials: credentials);\n"
    "\n"
    "        return new JwtSecurityTokenHandler().WriteToken(token);\n"
    "    }\n"
    "\n"
    "    public string GenerateRefreshToken() => Guid.NewGuid().ToString(\"N\");\n"
    "\n"
    "    public (Guid UserId, Guid TenantId, string Role) ValidateRefreshToken(string token)\n"
    "    {\n"
    "        var tokenHandler = new JwtSecurityTokenHandler();\n"
    "        var key = Encoding.UTF8.GetBytes(_opts.SecretKey);\n"
    "        var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters\n"
    "        {\n"
    "            ValidateIssuerSigningKey = true,\n"
    "            IssuerSigningKey = new SymmetricSecurityKey(key),\n"
    "            ValidateIssuer = false,\n"
    "            ValidateAudience = false,\n"
    "            ClockSkew = TimeSpan.Zero\n"
    "        }, out _);\n"
    "\n"
    "        var userId = Guid.Parse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub)!);\n"
    "        var tenantId = Guid.Parse(principal.FindFirstValue(\"tenant_id\")!);\n"
    "        var role = principal.FindFirstValue(ClaimTypes.Role)!;\n"
    "\n"
    "        return (userId, tenantId, role);\n"
    "    }\n"
    "}\n"
)
with open(jwt_path, 'w') as f:
    f.write(jwt_content)
print("OK: JwtTokenService.cs rewritten")

print("\nAll fixes applied.")
