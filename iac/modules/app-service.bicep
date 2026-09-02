@description('Name of the Web App. Must be globally unique across Azure (it becomes <name>.azurewebsites.net unless a custom domain is added).')
param name string

@description('Azure region for the app.')
param location string

@description('Resource ID of the App Service Plan (Linux) that hosts this app.')
param appServicePlanId string

@description('Linux runtime stack, e.g. "DOTNETCORE|10.0", "NODE|22-lts", "PYTHON|3.13".')
param linuxFxVersion string

@description('Optional startup command, e.g. "pm2 serve /home/site/wwwroot --no-daemon --spa --port 8080" for a static SPA.')
param appCommandLine string = ''

@description('Extra application settings (environment variables) for the app, as an array of { name, value }. Merged with a few sensible defaults.')
param appSettings array = []

@description('Keep the app loaded (no cold start after idle). Not available on the Free (F1) SKU.')
param alwaysOn bool = true

@description('Redirect all HTTP traffic to HTTPS.')
param httpsOnly bool = true

@description('Minimum TLS version accepted.')
param minTlsVersion string = '1.2'

@description('Optional health check path, e.g. "/healthz". Leave empty to disable.')
param healthCheckPath string = ''

@description('Enable a system-assigned managed identity on the app.')
param enableManagedIdentity bool = true

@description('Tags applied to the app.')
param tags object = {}

resource app 'Microsoft.Web/sites@2023-12-01' = {
  name: name
  location: location
  tags: tags
  kind: 'app,linux'
  identity: enableManagedIdentity
    ? {
        type: 'SystemAssigned'
      }
    : null
  properties: {
    serverFarmId: appServicePlanId
    httpsOnly: httpsOnly
    siteConfig: {
      linuxFxVersion: linuxFxVersion
      alwaysOn: alwaysOn
      minTlsVersion: minTlsVersion
      ftpsState: 'Disabled'
      appCommandLine: appCommandLine
      healthCheckPath: empty(healthCheckPath) ? null : healthCheckPath
      appSettings: appSettings
    }
  }
}

output id string = app.id
output name string = app.name
output defaultHostName string = app.properties.defaultHostName
output principalId string = enableManagedIdentity ? app.identity.principalId : ''
