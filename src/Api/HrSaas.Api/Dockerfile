FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files first for layer caching
COPY ["src/Api/HrSaas.Api/HrSaas.Api.csproj", "src/Api/HrSaas.Api/"]
COPY ["src/Modules/Employee/HrSaas.Modules.Employee/HrSaas.Modules.Employee.csproj", "src/Modules/Employee/HrSaas.Modules.Employee/"]
COPY ["src/Modules/Identity/HrSaas.Modules.Identity/HrSaas.Modules.Identity.csproj", "src/Modules/Identity/HrSaas.Modules.Identity/"]
COPY ["src/Modules/Tenant/HrSaas.Modules.Tenant/HrSaas.Modules.Tenant.csproj", "src/Modules/Tenant/HrSaas.Modules.Tenant/"]
COPY ["src/Modules/Leave/HrSaas.Modules.Leave/HrSaas.Modules.Leave.csproj", "src/Modules/Leave/HrSaas.Modules.Leave/"]
COPY ["src/Modules/Billing/HrSaas.Modules.Billing/HrSaas.Modules.Billing.csproj", "src/Modules/Billing/HrSaas.Modules.Billing/"]
COPY ["src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/HrSaas.SharedKernel.csproj", "src/BuildingBlocks/SharedKernel/HrSaas.SharedKernel/"]
COPY ["src/BuildingBlocks/TenantSdk/HrSaas.TenantSdk/HrSaas.TenantSdk.csproj", "src/BuildingBlocks/TenantSdk/HrSaas.TenantSdk/"]
COPY ["src/BuildingBlocks/EventBus/HrSaas.EventBus/HrSaas.EventBus.csproj", "src/BuildingBlocks/EventBus/HrSaas.EventBus/"]

RUN dotnet restore "src/Api/HrSaas.Api/HrSaas.Api.csproj"

# Copy all source
COPY . .

WORKDIR "/src/src/Api/HrSaas.Api"
RUN dotnet build "HrSaas.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "HrSaas.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
# Run as non-root user for security
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "HrSaas.Api.dll"]
