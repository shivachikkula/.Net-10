# Student API

A minimal-API Student CRUD-lite (create, update, get) service built on **.NET 10** / **C# 14**, backed by **SQL Server** via EF Core 10.

## Project layout

```
src/StudentApi.Api/          ASP.NET Core minimal API project
  Models/Student.cs           EF Core entity
  Data/StudentDbContext.cs    DbContext + model configuration
  Dtos/StudentDtos.cs         Create/Update request + response records
  Extensions/StudentMappingExtensions.cs   entity <-> DTO mapping (C# 14 extension members)
  Endpoints/StudentEndpoints.cs            route handlers
  Migrations/                 EF Core migrations
tests/StudentApi.Tests/       xUnit integration tests (WebApplicationFactory + EF InMemory)
```

## C# 14 / .NET 10 features used

- **`field` keyword** — `Student.Email` normalizes (trim + lowercase) on assignment without a hand-declared backing field (`Models/Student.cs`).
- **Extension members** (`extension(Type x) { ... }` blocks) — entity/DTO mapping in `StudentMappingExtensions.cs`.
- **Minimal API automatic validation** (new in .NET 10, `builder.Services.AddValidation()`) — `DataAnnotations` on the request DTOs are enforced automatically, no hand-written validation filter.
- **Primary constructors** — `StudentDbContext(DbContextOptions<StudentDbContext> options)`.
- **`TypedResults`/`Results<...>`** minimal API return types for strongly-typed OpenAPI metadata.
- **`DateOnly`** for date-of-birth, mapped to SQL Server `date`.

## Endpoints

| Method | Route                  | Description                    |
|--------|------------------------|---------------------------------|
| GET    | `/api/students`         | List all students               |
| GET    | `/api/students/{id}`    | Get a single student by id      |
| POST   | `/api/students`         | Create a student                |
| PUT    | `/api/students/{id}`    | Update an existing student      |

`POST`/`PUT` validate the payload (required fields, email format, GPA range 0-4) and return `409 Conflict` if the email is already taken by another student.

## Running against SQL Server

1. Update the `ConnectionStrings:StudentDb` value in `src/StudentApi.Api/appsettings.json` (or override via `appsettings.Development.json` / environment variables / user-secrets) to point at your SQL Server instance.
2. Run the API — in `Development`, it applies pending EF Core migrations automatically on startup:

   ```bash
   dotnet run --project src/StudentApi.Api
   ```

   Or apply migrations manually:

   ```bash
   dotnet tool install --global dotnet-ef
   dotnet ef database update --project src/StudentApi.Api
   ```

3. Browse the OpenAPI document at `/openapi/v1.json` in Development, or hit the endpoints directly, e.g.:

   ```bash
   curl -X POST http://localhost:5000/api/students \
     -H "Content-Type: application/json" \
     -d '{"firstName":"Ada","lastName":"Lovelace","email":"ada@example.com","dateOfBirth":"2000-12-10","department":"Mathematics","gpa":3.9}'
   ```

## Tests

```bash
dotnet test
```

Integration tests swap in EF Core's InMemory provider (see `StudentApiFactory`) so they run without a real SQL Server instance.

## CI/CD: deploy to Azure App Service

`.github/workflows/azure-webapps-deploy.yml` builds, tests, and publishes `src/StudentApi.Api`, then deploys it to an Azure App Service using a publish profile. It runs on every push to `main` and can also be triggered manually (Actions tab → the workflow → **Run workflow**).

Before it can deploy, set these in the repo's **Settings → Secrets and variables → Actions**:

- **Variables** tab → New repository variable:
  - `AZURE_WEBAPP_NAME` = the name of your Azure App Service (e.g. `student-api-prod`)
- **Secrets** tab → New repository secret:
  - `AZURE_WEBAPP_PUBLISH_PROFILE` = the full contents of the publish profile downloaded from Azure Portal → your App Service → **Overview → Get publish profile** (or `az webapp deployment list-publishing-profiles --xml`)

Notes:
- The App Service's stack should be set to .NET 10 (or "framework-dependent" hosting on a runtime that supports it); if your App Service can't yet run .NET 10, publish self-contained instead by adding `--self-contained true -r linux-x64` (or `win-x64`) to the `dotnet publish` step.
- The workflow deploys straight to production (no slot). Add a `slot-name` input to the `azure/webapps-deploy` step if you use deployment slots.
