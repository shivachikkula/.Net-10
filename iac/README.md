# Infrastructure as Code (Bicep)

Deploys the Azure resources this repo's apps run on: a resource group, one shared Linux
App Service Plan, and a Web App per application (API + client here) — all on that one plan.
It's written as small, reusable modules so you can point it at another project by writing a
new parameters file, without touching the Bicep itself.

## Layout

```
iac/
  main.bicep                    Orchestrator (subscription-scope): resource group + plan + apps
  modules/
    resource-group.bicep         Creates the resource group
    app-service-plan.bicep       Creates a Linux App Service Plan (SKU is a parameter)
    app-service.bicep            Creates one Linux Web App (runtime, startup command,
                                  app settings, health check, managed identity — all parameters)
  parameters/
    dev.bicepparam                Example environment: names, SKUs, runtimes, app settings
```

`main.bicep` deploys:
- **1** resource group
- **1** App Service Plan (shared by every app below)
- **2** named Web Apps: `apiApp` (.NET) and `clientApp` (Node, serving the Angular build)
- **N** more Web Apps from the `additionalApps` array parameter — this is the "reusable for
  other apps" part: add an entry instead of copying the template

## Why Bicep

Free (no extra tooling cost), native to Azure (no state file to store or lock, unlike
Terraform), and ships in the `az` CLI already used to deploy Azure resources.

## Prerequisites

- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) (`az`), which bundles Bicep
  (`az bicep install` if it's missing)
- An Azure subscription, and `Contributor` (or `Owner`) access on it — this template creates the
  resource group itself, so you need subscription-level access rather than just access to an
  existing resource group
- Logged in locally: `az login`

## Run it locally

```bash
cd iac

# 1. Copy and edit a parameters file for your environment (app names must be
#    globally unique across Azure — they become <name>.azurewebsites.net).
cp parameters/dev.bicepparam parameters/myenv.bicepparam
$EDITOR parameters/myenv.bicepparam

# 2. Preview what would change (safe, makes no changes).
az deployment sub what-if \
  --location eastus \
  --template-file main.bicep \
  --parameters parameters/myenv.bicepparam

# 3. Deploy.
az deployment sub create \
  --name iac-myenv-$(date +%s) \
  --location eastus \
  --template-file main.bicep \
  --parameters parameters/myenv.bicepparam

# 4. Read back the app hostnames it created.
az deployment sub show --name <name-from-step-3> \
  --query "properties.outputs.{api:apiApp.value,client:clientApp.value}"
```

`--location` above is where the *deployment record* lives, independent of the resource
`location` parameter inside the parameters file — most people just set both to the same region.

### Tear down

```bash
az group delete --name <resourceGroupName> --yes --no-wait
```

## Using this for another app

1. Copy `parameters/dev.bicepparam` to a new file.
2. Set unique names for `resourceGroupName`, `appServicePlanName`, and every app's `name`.
3. Set each app's `linuxFxVersion` to its runtime, e.g.:
   - .NET: `DOTNETCORE|10.0`, `DOTNETCORE|8.0`
   - Node: `NODE|22-lts`, `NODE|20-lts`
   - Python: `PYTHON|3.13`
   - Java: `JAVA|21-java21`
4. If the app is a static SPA build (like the Angular client), set `appCommandLine` to
   `pm2 serve /home/site/wwwroot --no-daemon --spa --port 8080`. Leave it empty for anything
   that runs its own server (an ASP.NET Core app, an Express server, etc.).
5. Need a third (or fourth, ...) app? Add it to the `additionalApps` array instead of editing
   `main.bicep`:
   ```bicep
   param additionalApps = [
     {
       name: 'my-worker-dev'
       linuxFxVersion: 'DOTNETCORE|10.0'
     }
   ]
   ```
6. Run the `what-if` / `create` commands above against the new parameters file.

Each app object accepts: `name`, `linuxFxVersion` (both required), and optionally
`appCommandLine`, `appSettings` (array of `{ name, value }`), `alwaysOn` (default `true`),
and `healthCheckPath`.

## Secrets and connection strings

Don't put secrets (connection strings, API keys) in a `.bicepparam` file — they'd end up in
git history. Set them after deploying instead:

```bash
az webapp config connection-string set \
  --name <apiApp.name> --resource-group <resourceGroupName> \
  --connection-string-type SQLAzure \
  --settings StudentDb="Server=...;Database=...;..."
```

or as a plain app setting (`az webapp config appsettings set --settings Key=Value`), or wire up
Key Vault references once you need something more robust.

## GitHub Actions

`.github/workflows/iac-deploy.yml` runs this template from CI:

- **Pull requests** touching `iac/**` run `az deployment sub what-if` (read-only preview,
  visible in the job log) using the `dev` parameters file, or whichever `environment` was
  specified.
- **Pushes to `main`** touching `iac/**` deploy the `dev` environment automatically
  (`az deployment sub create`).
- **Manual runs** (Actions tab → *Deploy Azure infrastructure (Bicep)* → **Run workflow**) accept
  an `environment` input (`dev`, `staging`, `prod`, ...) matching a
  `iac/parameters/<environment>.bicepparam` file — this is how you deploy anything other than
  `dev`, and how you re-run a deployment on demand.

The job for each `environment` input targets a matching **GitHub Environment** of the same
name — add required reviewers to the `staging`/`prod` GitHub Environments (Settings →
Environments) if you want a manual approval gate before those apply; `dev` can stay ungated.

### One-time setup: OIDC login (no stored client secret)

Applying this template needs real Azure RBAC (it creates resource groups, plans, and apps),
unlike the app-deploy workflows which only need a publish profile. The workflow logs in via
[OIDC federation](https://learn.microsoft.com/azure/developer/github/connect-from-azure) —
GitHub proves its identity to Azure AD for each run, so no long-lived secret is stored.

```bash
# 1. Create an app registration (or reuse one) and a service principal for it.
az ad app create --display-name student-app-github-actions
appId=$(az ad app list --display-name student-app-github-actions --query "[0].appId" -o tsv)
az ad sp create --id "$appId"

# 2. Give it Contributor on the subscription (needed because this template
#    creates the resource group itself; scope to an existing RG instead if
#    you'd rather not grant subscription-wide access).
az role assignment create \
  --assignee "$appId" \
  --role Contributor \
  --scope "/subscriptions/<subscription-id>"

# 3. Add a federated credential trusting this repo's main branch.
az ad app federated-credential create --id "$appId" --parameters '{
  "name": "github-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:<owner>/<repo>:ref:refs/heads/main",
  "audiences": ["api://AzureADTokenExchange"]
}'

# Repeat step 3 with a different "name"/"subject" for pull_request runs, e.g.:
#   "subject": "repo:<owner>/<repo>:pull_request"
# and for each GitHub Environment you gate ("dev"/"staging"/"prod"), e.g.:
#   "subject": "repo:<owner>/<repo>:environment:dev"
```

Then, in the repo's **Settings → Secrets and variables → Actions**:

- **Secrets**: `AZURE_CLIENT_ID` (the app registration's `appId`), `AZURE_TENANT_ID`,
  `AZURE_SUBSCRIPTION_ID`
- **Variables** *(optional)*: `AZURE_LOCATION` — the region for the deployment record itself
  (default `eastus` if unset)
