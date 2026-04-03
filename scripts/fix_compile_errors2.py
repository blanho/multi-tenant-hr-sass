import os
import re
import glob

base = "/Users/macbook/Desktop/multi-tenant-sass"

# ── Fix 1: Employee module – use Domain.Entities.Employee (relative) instead of bare Employee ──

# 1a. IEmployeeRepository.cs
path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Application/Interfaces/IEmployeeRepository.cs"
with open(path, 'w') as f:
    f.write(
        "namespace HrSaas.Modules.Employee.Application.Interfaces;\n"
        "\n"
        "public interface IEmployeeRepository\n"
        "{\n"
        "    Task<Domain.Entities.Employee?> GetByIdAsync(Guid id, CancellationToken ct = default);\n"
        "\n"
        "    Task<IReadOnlyList<Domain.Entities.Employee>> GetAllAsync(CancellationToken ct = default);\n"
        "\n"
        "    Task<IReadOnlyList<Domain.Entities.Employee>> GetByDepartmentAsync(\n"
        "        string department,\n"
        "        CancellationToken ct = default);\n"
        "\n"
        "    Task AddAsync(Domain.Entities.Employee employee, CancellationToken ct = default);\n"
        "\n"
        "    void Update(Domain.Entities.Employee employee);\n"
        "\n"
        "    void Delete(Domain.Entities.Employee employee);\n"
        "\n"
        "    Task<int> SaveChangesAsync(CancellationToken ct = default);\n"
        "}\n"
    )
print("OK: IEmployeeRepository.cs")

# 1b. IEmployeeDbContext.cs
path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Application/Interfaces/IEmployeeDbContext.cs"
with open(path, 'w') as f:
    f.write(
        "using Microsoft.EntityFrameworkCore;\n"
        "\n"
        "namespace HrSaas.Modules.Employee.Application.Interfaces;\n"
        "\n"
        "public interface IEmployeeDbContext\n"
        "{\n"
        "    DbSet<Domain.Entities.Employee> Employees { get; }\n"
        "    Task<int> SaveChangesAsync(CancellationToken ct = default);\n"
        "}\n"
    )
print("OK: IEmployeeDbContext.cs")

# 1c. EmployeeDbContext.cs – add using for Application.Interfaces
path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Infrastructure/Persistence/EmployeeDbContext.cs"
with open(path, 'r') as f:
    content = f.read()
if "using HrSaas.Modules.Employee.Application.Interfaces;" not in content:
    content = "using HrSaas.Modules.Employee.Application.Interfaces;\n" + content
    with open(path, 'w') as f:
        f.write(content)
print("OK: EmployeeDbContext.cs using added")

# 1d. EmployeeRepository.cs – fix Delete implementation to use Domain.Entities.Employee
path = f"{base}/src/Modules/Employee/HrSaas.Modules.Employee/Infrastructure/Persistence/Repositories/EmployeeRepository.cs"
with open(path, 'r') as f:
    content = f.read()
# Fix Delete method if it used wrong type
content = content.replace(
    "    public void Delete(Employee employee)\n"
    "    {\n"
    "        _dbContext.Employees.Remove(employee);\n"
    "    }",
    "    public void Delete(Domain.Entities.Employee employee)\n"
    "    {\n"
    "        dbContext.Employees.Remove(employee);\n"
    "    }"
)
with open(path, 'w') as f:
    f.write(content)
print("OK: EmployeeRepository.cs Delete method fixed")

# ── Fix 2: Tenant module – apply TenantEntity alias to ALL files in the module ──

tenant_module = f"{base}/src/Modules/Tenant/HrSaas.Modules.Tenant"
all_tenant_cs = glob.glob(f"{tenant_module}/**/*.cs", recursive=True)

old_using = "using HrSaas.Modules.Tenant.Domain.Entities;"
new_using = "using TenantEntity = HrSaas.Modules.Tenant.Domain.Entities.Tenant;"

for fpath in all_tenant_cs:
    with open(fpath, 'r') as f:
        content = f.read()
    
    changed = False
    if old_using in content and new_using not in content:
        content = content.replace(old_using, new_using)
        # Replace Tenant type usages (not namespace parts like HrSaas.Modules.Tenant.X)
        # Replace 'Tenant?' -> 'TenantEntity?'
        content = re.sub(r'(?<![.\w])Tenant\?', 'TenantEntity?', content)
        # Replace '<Tenant>' -> '<TenantEntity>'
        content = content.replace('<Tenant>', '<TenantEntity>')
        content = content.replace('<Tenant,', '<TenantEntity,')
        # Replace 'IEntityTypeConfiguration<...' handled above
        content = content.replace('IEntityTypeConfiguration<TenantEntity>', 'IEntityTypeConfiguration<TenantEntity>')
        # Replace method signatures: Tenant tenant -> TenantEntity tenant
        content = re.sub(r'(?<![.\w])Tenant (?=tenant\b)', 'TenantEntity ', content)
        # Replace AddAsync(Tenant tenant -> AddAsync(TenantEntity tenant
        content = re.sub(r'\(Tenant (?=tenant\b)', '(TenantEntity ', content)
        # Replace DbSet<Tenant>
        content = content.replace('DbSet<Tenant>', 'DbSet<TenantEntity>')
        # Replace EntityTypeBuilder<Tenant>
        content = content.replace('EntityTypeBuilder<Tenant>', 'EntityTypeBuilder<TenantEntity>')
        # Replace IEntityTypeConfiguration<Tenant>
        content = content.replace('IEntityTypeConfiguration<Tenant>', 'IEntityTypeConfiguration<TenantEntity>')
        # Replace 'Tenant.Create' and 'Tenant.X()' with 'TenantEntity.X()'
        content = re.sub(r'(?<![.\w])Tenant\.', 'TenantEntity.', content)
        changed = True

    if changed:
        with open(fpath, 'w') as f:
            f.write(content)
        print(f"OK: {os.path.relpath(fpath, tenant_module)}")

# ── Fix 3: AppUser.cs – add IsActive property ──
appuser_path = f"{base}/src/Modules/Identity/HrSaas.Modules.Identity/Domain/Entities/AppUser.cs"
with open(appuser_path, 'r') as f:
    content = f.read()

if "public bool IsActive" not in content:
    content = content.replace(
        "    public string Role { get; private set; } = null!;",
        "    public string Role { get; private set; } = null!;\n    public bool IsActive { get; private set; } = true;"
    )
    # Fix Deactivate/Activate to use Touch()
    content = content.replace(
        "    public void Deactivate()\n"
        "    {\n"
        "        IsActive = false;\n"
        "        UpdatedAt = DateTime.UtcNow;\n"
        "        AddDomainEvent(new UserDeactivatedEvent(TenantId, Id));\n"
        "    }",
        "    public void Deactivate()\n"
        "    {\n"
        "        IsActive = false;\n"
        "        Touch();\n"
        "        AddDomainEvent(new UserDeactivatedEvent(TenantId, Id));\n"
        "    }"
    )
    content = content.replace(
        "    public void Activate()\n"
        "    {\n"
        "        IsActive = true;\n"
        "        UpdatedAt = DateTime.UtcNow;\n"
        "    }",
        "    public void Activate()\n"
        "    {\n"
        "        IsActive = true;\n"
        "        Touch();\n"
        "    }"
    )
    # Also fix ChangeRole to use Touch()
    content = content.replace(
        "        UpdatedAt = DateTime.UtcNow;\n"
        "        AddDomainEvent(new UserRoleChangedEvent",
        "        Touch();\n"
        "        AddDomainEvent(new UserRoleChangedEvent"
    )
    with open(appuser_path, 'w') as f:
        f.write(content)
    print("OK: AppUser.cs IsActive added")
else:
    print("OK: AppUser.cs IsActive already present")

print("\nAll fixes applied.")
