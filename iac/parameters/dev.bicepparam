using '../main.bicep'

// Copy this file per environment (dev.bicepparam, staging.bicepparam, prod.bicepparam, ...)
// and adjust the values below. App names must be globally unique across Azure.

param resourceGroupName = 'rg-student-app-dev'
param location = 'eastus'

param appServicePlanName = 'plan-student-app-dev'
param appServicePlanSku = 'B1'

param apiApp = {
  name: 'student-api-dev' // -> https://student-api-dev.azurewebsites.net
  linuxFxVersion: 'DOTNETCORE|10.0'
  healthCheckPath: '/api/students'
  appSettings: [
    {
      name: 'ASPNETCORE_ENVIRONMENT'
      value: 'Development'
    }
    {
      name: 'Cors__AllowedOrigins__0'
      value: 'https://student-client-dev.azurewebsites.net'
    }
    // Add the real connection string as a secret app setting once the
    // database exists, e.g. via `az webapp config connection-string set`,
    // rather than committing it here:
    // { name: 'ConnectionStrings__StudentDb', value: '...' }
  ]
}

param clientApp = {
  name: 'student-client-dev' // -> https://student-client-dev.azurewebsites.net
  linuxFxVersion: 'NODE|22-lts'
  // Serves the Angular static build with SPA fallback routing.
  appCommandLine: 'pm2 serve /home/site/wwwroot --no-daemon --spa --port 8080'
}

// Add more apps here later without touching main.bicep, e.g.:
// param additionalApps = [
//   {
//     name: 'student-worker-dev'
//     linuxFxVersion: 'DOTNETCORE|10.0'
//   }
// ]
param additionalApps = []

param tags = {
  environment: 'dev'
  project: 'student-app'
  managedBy: 'bicep'
}
