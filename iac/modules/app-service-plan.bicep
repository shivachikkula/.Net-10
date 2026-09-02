@description('Name of the App Service Plan.')
param name string

@description('Azure region for the plan.')
param location string

@description('SKU name, e.g. F1 (free), B1/B2/B3 (basic), P0v3/P1v3 (premium v3).')
param skuName string = 'B1'

@description('Number of workers/instances.')
param capacity int = 1

@description('true for Linux (this repo only uses Linux app services), false for Windows.')
param reserved bool = true

@description('Tags applied to the plan.')
param tags object = {}

resource plan 'Microsoft.Web/serverfarms@2023-12-01' = {
  name: name
  location: location
  tags: tags
  sku: {
    name: skuName
    capacity: capacity
  }
  kind: reserved ? 'linux' : 'app'
  properties: {
    reserved: reserved
  }
}

output id string = plan.id
output name string = plan.name
