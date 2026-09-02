targetScope = 'subscription'

@description('Name of the resource group to create (or reuse) for these resources.')
param resourceGroupName string

@description('Azure region for every resource.')
param location string = 'eastus'

@description('Name of the shared Linux App Service Plan hosting all the apps below.')
param appServicePlanName string

@description('App Service Plan SKU, e.g. F1 (free), B1/B2/B3 (basic), P0v3/P1v3 (premium v3).')
param appServicePlanSku string = 'B1'

@description('''
API app configuration. Only "name" and "linuxFxVersion" are required; everything else has a
default. Shape: { name, linuxFxVersion, appCommandLine?, appSettings?, alwaysOn?, healthCheckPath? }
''')
param apiApp object

@description('''
Client app configuration. Same shape as apiApp. Defaults its appCommandLine to a `pm2 serve`
command so a static Angular/React/Vue build works out of the box on a Linux Node app service.
''')
param clientApp object

@description('''
Any additional apps to deploy onto the same plan, each with the same shape as apiApp/clientApp.
This is what makes the template reusable for other projects: add an entry here instead of copying
the whole template for one more app.
''')
param additionalApps array = []

@description('Tags applied to every resource.')
param tags object = {}

module rg 'modules/resource-group.bicep' = {
  name: 'resource-group'
  params: {
    name: resourceGroupName
    location: location
    tags: tags
  }
}

module appServicePlan 'modules/app-service-plan.bicep' = {
  name: 'app-service-plan'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: appServicePlanName
    location: location
    skuName: appServicePlanSku
    tags: tags
  }
  dependsOn: [
    rg
  ]
}

module apiAppService 'modules/app-service.bicep' = {
  name: 'api-app-service'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: apiApp.name
    location: location
    appServicePlanId: appServicePlan.outputs.id
    linuxFxVersion: apiApp.linuxFxVersion
    appCommandLine: apiApp.?appCommandLine ?? ''
    appSettings: apiApp.?appSettings ?? []
    alwaysOn: apiApp.?alwaysOn ?? true
    healthCheckPath: apiApp.?healthCheckPath ?? ''
    tags: tags
  }
}

module clientAppService 'modules/app-service.bicep' = {
  name: 'client-app-service'
  scope: resourceGroup(resourceGroupName)
  params: {
    name: clientApp.name
    location: location
    appServicePlanId: appServicePlan.outputs.id
    linuxFxVersion: clientApp.linuxFxVersion
    appCommandLine: clientApp.?appCommandLine ?? 'pm2 serve /home/site/wwwroot --no-daemon --spa --port 8080'
    appSettings: clientApp.?appSettings ?? []
    alwaysOn: clientApp.?alwaysOn ?? true
    healthCheckPath: clientApp.?healthCheckPath ?? ''
    tags: tags
  }
}

module additionalAppServices 'modules/app-service.bicep' = [
  for app in additionalApps: {
    name: 'app-service-${app.name}'
    scope: resourceGroup(resourceGroupName)
    params: {
      name: app.name
      location: location
      appServicePlanId: appServicePlan.outputs.id
      linuxFxVersion: app.linuxFxVersion
      appCommandLine: app.?appCommandLine ?? ''
      appSettings: app.?appSettings ?? []
      alwaysOn: app.?alwaysOn ?? true
      healthCheckPath: app.?healthCheckPath ?? ''
      tags: tags
    }
  }
]

output resourceGroupName string = resourceGroupName
output appServicePlanId string = appServicePlan.outputs.id

output apiApp object = {
  name: apiAppService.outputs.name
  defaultHostName: apiAppService.outputs.defaultHostName
}

output clientApp object = {
  name: clientAppService.outputs.name
  defaultHostName: clientAppService.outputs.defaultHostName
}

output additionalApps array = [
  for i in range(0, length(additionalApps)): {
    name: additionalAppServices[i].outputs.name
    defaultHostName: additionalAppServices[i].outputs.defaultHostName
  }
]
