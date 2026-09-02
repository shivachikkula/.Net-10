# Student API

A minimal-API Student CRUD service built on **.NET 10** / **C# 14**, backed by **SQL Server** via EF Core 10, with an **Angular 22** + **Tailwind CSS** front end.

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
client/                       Angular 22 + Tailwind CSS front end (see below)
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
| DELETE | `/api/students/{id}`    | Remove a student                |

`POST`/`PUT` validate the payload (required fields, email format, GPA range 0-4) and return `409 Conflict` if the email is already taken by another student.

CORS is enabled for the Angular dev server (`http://localhost:4200` by default — see the `Cors:AllowedOrigins` setting in `appsettings.json`) so the client below can call the API locally.

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

## Angular front end (`client/`)

A single-page app for listing, creating, editing, and removing students, built with **Angular 22** (standalone, zoneless, signals) and **Tailwind CSS v4**.

```
client/src/app/
  models/student.ts                     Student / StudentFormValue types
  services/student.ts                   StudentService (@Service) — HttpClient calls + signal-based state
  components/student-grid/              Grid listing students, with Edit/Remove actions per row
  components/student-form-dialog/       Popup modal (reactive form) used for both Add and Edit
  app.ts / app.html                      Page shell: header, grid, and the dialog
```

Notable Angular 22 features used:
- **Zoneless change detection** (no `zone.js`) with the new `@angular/build:application` builder.
- **`@Service()`** — the new decorator that replaces `@Injectable({ providedIn: 'root' })`.
- Signal-based **`input()`/`output()`** on all components, and the **`@if`/`@for`** control-flow syntax.
- **Tailwind CSS v4** wired in via `@tailwindcss/postcss` (`ng new --style=tailwind`), so utility classes work with no extra config.

Add/Edit both open the same `StudentFormDialog` as a centered popup (overlay + backdrop click / Cancel to dismiss); Remove asks for confirmation before calling the API.

### Running the client

```bash
cd client
npm install
npm start          # ng serve, http://localhost:4200
```

The dev build points at `http://localhost:5123/api` (see `src/environments/environment.development.ts`) — start the API (`dotnet run --project src/StudentApi.Api`) alongside it. The production build (`npm run build`) uses `src/environments/environment.ts`, which defaults to a same-origin `/api` (adjust it, or front both apps with a reverse proxy, for your deployment).

### Lint & tests

```bash
cd client
npm run lint        # ESLint (typescript-eslint + angular-eslint)
npm test             # Vitest, via `ng test`
```

## CI/CD: deploy to Azure App Service

Two workflows, one per app, each using a **publish profile** (no Azure login/service principal needed):

| Workflow | Builds & deploys | Triggers on push to `main` when |
|---|---|---|
| `.github/workflows/azure-webapps-deploy.yml` | `src/StudentApi.Api` | `src/**`, `tests/**`, or `StudentApi.slnx` change |
| `.github/workflows/azure-webapps-deploy-client.yml` | `client/` (Angular production build) | `client/**` changes |

Both can also be triggered manually (Actions tab → the workflow → **Run workflow**).

Before they can deploy, set these in the repo's **Settings → Secrets and variables → Actions**:

- **Variables** tab:
  - `AZURE_WEBAPP_NAME` = the name of the API's Azure App Service (e.g. `student-api-prod`)
  - `AZURE_WEBAPP_NAME_CLIENT` = the name of the client's Azure App Service (e.g. `student-client-prod`)
  - `CLIENT_API_BASE_URL` *(optional)* = the API's public URL plus `/api` (e.g. `https://student-api-prod.azurewebsites.net/api`) — written into the client's production `environment.ts` before it's built, so the deployed client calls the deployed API instead of the committed `/api` default. Also update the API's `Cors:AllowedOrigins` (via an App Service application setting `Cors__AllowedOrigins__0`) to the client's URL so the browser isn't blocked by CORS.
- **Secrets** tab:
  - `AZURE_WEBAPP_PUBLISH_PROFILE` = publish profile for the API's App Service
  - `AZURE_WEBAPP_PUBLISH_PROFILE_CLIENT` = publish profile for the client's App Service

Get a publish profile from Azure Portal → the App Service → **Overview → Get publish profile** (or `az webapp deployment list-publishing-profiles --xml`).

Notes:
- **API**: the App Service's stack should be set to .NET 10 (or "framework-dependent" hosting on a runtime that supports it); if it can't yet run .NET 10, publish self-contained instead by adding `--self-contained true -r linux-x64` (or `win-x64`) to the `dotnet publish` step.
- **Client**: Angular's production build is static files (HTML/JS/CSS), so the client's App Service needs something to serve them — set its stack to **Node** and its **Startup Command** (Configuration → General settings) to:
  ```
  pm2 serve /home/site/wwwroot --no-daemon --spa --port 8080
  ```
  (`pm2` ships on Azure's Linux Node images; `--spa` falls back to `index.html` for client-side routes.)
- Both workflows deploy straight to production (no slot). Add a `slot-name` input to the `azure/webapps-deploy` step if you use deployment slots.

## Code scanning (CodeQL)

`.github/workflows/codeql-analysis.yml` runs [CodeQL](https://codeql.github.com/) — GitHub's static analysis engine, free for public repositories (this one included) — against both the API (`csharp`) and the client (`javascript-typescript`) on every push and pull request to `main`.

- Findings show up as **pull request annotations** on the offending lines and under the repo's **Security → Code scanning** tab.
- Unlike CodeQL's default behavior, this workflow **fails the job** if the scan finds anything at all (see the "Fail the build if issues were found" step, which counts the SARIF results) — so a merge with any flagged issue shows a red ✗ check rather than just a silent alert.
