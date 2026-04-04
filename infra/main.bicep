targetScope = 'resourceGroup'

@description('Environment name (dev, staging, prod)')
param environmentName string

@description('Azure region for all resources')
param location string = resourceGroup().location

@description('PostgreSQL administrator password')
@secure()
param postgresPassword string

@description('JWT signing key')
@secure()
param jwtSecretKey string

var prefix = 'hrsaas-${environmentName}'
var isProd = environmentName == 'prod'
var useSlots = isProd || environmentName == 'staging'
var tags = {
  project: 'HrSaas'
  environment: environmentName
  managedBy: 'bicep'
}

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: '${prefix}-logs'
  location: location
  tags: tags
  properties: {
    sku: { name: 'PerGB2018' }
    retentionInDays: 30
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: '${prefix}-insights'
  location: location
  tags: tags
  kind: 'web'
  properties: {
    Application_Type: 'web'
    WorkspaceResourceId: logAnalytics.id
  }
}

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: '${prefix}-kv'
  location: location
  tags: tags
  properties: {
    sku: { family: 'A', name: 'standard' }
    tenantId: subscription().tenantId
    enableRbacAuthorization: true
    enableSoftDelete: true
    softDeleteRetentionInDays: 7
  }
}

resource kvSecretJwt 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Jwt--SecretKey'
  properties: { value: jwtSecretKey }
}

resource kvSecretPostgres 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--DefaultConnection'
  properties: {
    value: 'Host=${postgresServer.properties.fullyQualifiedDomainName};Port=5432;Database=hrsaas;Username=hrsaas_admin;Password=${postgresPassword};Ssl Mode=Require'
  }
}

resource kvSecretServiceBus 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'ConnectionStrings--AzureServiceBus'
  properties: {
    value: listKeys(serviceBusAuthRule.id, serviceBusAuthRule.apiVersion).primaryConnectionString
  }
}

resource kvSecretBlob 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: 'Azure--BlobStorage--ConnectionString'
  properties: {
    value: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value}'
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: replace('${prefix}stor', '-', '')
  location: location
  tags: tags
  sku: { name: 'Standard_LRS' }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    supportsHttpsTrafficOnly: true
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
  properties: {
    deleteRetentionPolicy: { enabled: true, days: 7 }
  }
}

resource postgresServer 'Microsoft.DBforPostgreSQL/flexibleServers@2023-12-01-preview' = {
  name: '${prefix}-pg'
  location: location
  tags: tags
  sku: {
    name: environmentName == 'prod' ? 'Standard_D4ds_v5' : 'Standard_B1ms'
    tier: environmentName == 'prod' ? 'GeneralPurpose' : 'Burstable'
  }
  properties: {
    version: '16'
    administratorLogin: 'hrsaas_admin'
    administratorLoginPassword: postgresPassword
    storage: { storageSizeGB: environmentName == 'prod' ? 128 : 32 }
    backup: {
      backupRetentionDays: environmentName == 'prod' ? 35 : 7
      geoRedundantBackup: environmentName == 'prod' ? 'Enabled' : 'Disabled'
    }
    highAvailability: {
      mode: environmentName == 'prod' ? 'ZoneRedundant' : 'Disabled'
    }
  }
}

resource postgresDb 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2023-12-01-preview' = {
  parent: postgresServer
  name: 'hrsaas'
  properties: { charset: 'UTF8', collation: 'en_US.utf8' }
}

resource postgresFirewall 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2023-12-01-preview' = {
  parent: postgresServer
  name: 'AllowAzureServices'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

resource serviceBusNamespace 'Microsoft.ServiceBus/namespaces@2024-01-01' = {
  name: '${prefix}-sb'
  location: location
  tags: tags
  sku: {
    name: environmentName == 'prod' ? 'Premium' : 'Standard'
    tier: environmentName == 'prod' ? 'Premium' : 'Standard'
  }
}

resource serviceBusAuthRule 'Microsoft.ServiceBus/namespaces/AuthorizationRules@2024-01-01' = {
  parent: serviceBusNamespace
  name: 'hrsaas-app'
  properties: { rights: ['Listen', 'Send', 'Manage'] }
}

resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: '${prefix}-redis'
  location: location
  tags: tags
  properties: {
    sku: {
      name: environmentName == 'prod' ? 'Standard' : 'Basic'
      family: 'C'
      capacity: environmentName == 'prod' ? 1 : 0
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: '${prefix}-plan'
  location: location
  tags: tags
  kind: 'linux'
  sku: {
    name: isProd ? 'P1v3' : (environmentName == 'staging' ? 'S1' : 'B1')
    tier: isProd ? 'PremiumV3' : (environmentName == 'staging' ? 'Standard' : 'Basic')
  }
  properties: { reserved: true }
}

resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: '${prefix}-api'
  location: location
  tags: tags
  kind: 'app,linux'
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      healthCheckPath: '/health/live'
      http20Enabled: true
      ftpsState: 'Disabled'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: isProd ? 'Production' : 'Staging' }
        { name: 'Azure__KeyVault__VaultUri', value: keyVault.properties.vaultUri }
        { name: 'Azure__ApplicationInsights__ConnectionString', value: appInsights.properties.ConnectionString }
        { name: 'ConnectionStrings__Redis', value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False' }
        { name: 'Jwt__Issuer', value: 'HrSaas' }
        { name: 'Jwt__Audience', value: 'hrsaas-api' }
        { name: 'Telemetry__ServiceName', value: 'HrSaas.Api' }
        { name: 'Storage__Provider', value: 'AzureBlob' }
        { name: 'Database__AutoMigrate', value: 'false' }
      ]
      autoHealEnabled: isProd
      autoHealRules: isProd ? {
        triggers: {
          statusCodes: [
            { status: 500, subStatus: 0, count: 10, timeInterval: '00:05:00' }
          ]
          slowRequests: { timeTaken: '00:00:30', count: 5, timeInterval: '00:05:00' }
        }
        actions: { actionType: 'Recycle', minProcessExecutionTime: '00:02:00' }
      } : null
    }
  }
}

resource slotConfigNames 'Microsoft.Web/sites/config@2023-12-01' = if (useSlots) {
  parent: appService
  name: 'slotConfigNames'
  properties: {
    appSettingNames: [
      'Database__AutoMigrate'
    ]
  }
}

resource stagingSlot 'Microsoft.Web/sites/slots@2023-12-01' = if (useSlots) {
  parent: appService
  name: 'staging'
  location: location
  tags: tags
  identity: { type: 'SystemAssigned' }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|9.0'
      alwaysOn: true
      minTlsVersion: '1.2'
      healthCheckPath: '/health/live'
      http20Enabled: true
      ftpsState: 'Disabled'
      appSettings: [
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Staging' }
        { name: 'Azure__KeyVault__VaultUri', value: keyVault.properties.vaultUri }
        { name: 'Azure__ApplicationInsights__ConnectionString', value: appInsights.properties.ConnectionString }
        { name: 'ConnectionStrings__Redis', value: '${redisCache.properties.hostName}:${redisCache.properties.sslPort},password=${redisCache.listKeys().primaryKey},ssl=True,abortConnect=False' }
        { name: 'Jwt__Issuer', value: 'HrSaas' }
        { name: 'Jwt__Audience', value: 'hrsaas-api' }
        { name: 'Telemetry__ServiceName', value: 'HrSaas.Api' }
        { name: 'Storage__Provider', value: 'AzureBlob' }
        { name: 'Database__AutoMigrate', value: 'true' }
      ]
    }
  }
}

resource kvAccessApp 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, appService.id, '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource kvAccessSlot 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (useSlots) {
  name: guid(keyVault.id, '${prefix}-slot', '4633458b-17de-408a-b874-0445c86b69e6')
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', '4633458b-17de-408a-b874-0445c86b69e6')
    principalId: useSlots ? stagingSlot.identity.principalId : appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource blobAccessApp 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, appService.id, 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource blobAccessSlot 'Microsoft.Authorization/roleAssignments@2022-04-01' = if (useSlots) {
  name: guid(storageAccount.id, '${prefix}-slot', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe')
    principalId: useSlots ? stagingSlot.identity.principalId : appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource autoScale 'Microsoft.Insights/autoscalesettings@2022-10-01' = if (isProd) {
  name: '${prefix}-autoscale'
  location: location
  tags: tags
  properties: {
    targetResourceUri: appServicePlan.id
    enabled: true
    profiles: [
      {
        name: 'auto-scale-default'
        capacity: { minimum: '2', maximum: '10', default: '2' }
        rules: [
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricResourceUri: appServicePlan.id
              operator: 'GreaterThan'
              threshold: 70
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT5M'
              statistic: 'Average'
            }
            scaleAction: { direction: 'Increase', type: 'ChangeCount', value: '1', cooldown: 'PT5M' }
          }
          {
            metricTrigger: {
              metricName: 'MemoryPercentage'
              metricResourceUri: appServicePlan.id
              operator: 'GreaterThan'
              threshold: 80
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT5M'
              statistic: 'Average'
            }
            scaleAction: { direction: 'Increase', type: 'ChangeCount', value: '1', cooldown: 'PT5M' }
          }
          {
            metricTrigger: {
              metricName: 'CpuPercentage'
              metricResourceUri: appServicePlan.id
              operator: 'LessThan'
              threshold: 25
              timeAggregation: 'Average'
              timeGrain: 'PT1M'
              timeWindow: 'PT10M'
              statistic: 'Average'
            }
            scaleAction: { direction: 'Decrease', type: 'ChangeCount', value: '1', cooldown: 'PT10M' }
          }
        ]
      }
    ]
  }
}

resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${prefix}-api-diag'
  scope: appService
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      { category: 'AppServiceHTTPLogs', enabled: true }
      { category: 'AppServiceConsoleLogs', enabled: true }
      { category: 'AppServiceAppLogs', enabled: true }
      { category: 'AppServicePlatformLogs', enabled: true }
    ]
    metrics: [
      { category: 'AllMetrics', enabled: true }
    ]
  }
}

resource postgresDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${prefix}-pg-diag'
  scope: postgresServer
  properties: {
    workspaceId: logAnalytics.id
    logs: [
      { category: 'PostgreSQLLogs', enabled: true }
    ]
    metrics: [
      { category: 'AllMetrics', enabled: true }
    ]
  }
}

output appServiceName string = appService.name
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output stagingSlotUrl string = useSlots ? 'https://${stagingSlot.properties.defaultHostName}' : ''
output keyVaultUri string = keyVault.properties.vaultUri
output appInsightsConnectionString string = appInsights.properties.ConnectionString
output postgresHost string = postgresServer.properties.fullyQualifiedDomainName
output serviceBusNamespaceName string = serviceBusNamespace.name
output storageAccountName string = storageAccount.name
output redisCacheHost string = redisCache.properties.hostName
