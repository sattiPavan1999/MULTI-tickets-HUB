# Database Schema & Migrations Guide

This guide covers how to create schemas and migrations for every microservice module using Entity Framework Core Code-First with PostgreSQL (Npgsql).

---

## Overview

Each module owns:
- A **DbContext** class (e.g. `ConfigDbContext`) that defines tables, relationships, indexes, views, and seed data
- A **Migrations** folder where EF generates versioned migration files
- A **settings key** in `appsettings.json` that supplies the connection string and schema name
- An **EF bundle** (compiled binary) used in CI/CD to apply migrations without running the app

The seven modules and their schemas are:

| Module | DbContext | Schema | Settings Key | Core Project Path |
|---|---|---|---|---|
| Config | `ConfigDbContext` | `config` | `ConfigSettings:Core` | `Config/Backend/Src/Config.Core` |
| Identity | `IdentityDbContext` | `identity` | `IdentitySettings:Core` | `Identity/Backend/Src/Identity.Core` |
| Provider | `ProviderDbContext` | `provider` | `ProviderSettings:Core` | `Provider/Backend/Src/Provider.Core` |
| Workflow | `WorkflowDbContext` | `workflow` | `WorkflowSettings:Core` | `Workflow/Backend/Src/Workflow.Core` |
| Resource | `ResourceDbContext` | `resource` | `ResourceSettings:Core` | `Resource/Backend/Src/Resource.Core` |
| Finance | `FinanceDbContext` | `finance` | `FinanceSettings:Core` | `Finance/Backend/Src/Finance.Core` |
| Messaging | `MessagingDbContext` | `messaging` | `MessagingSettings:Core` | `Messaging/Backend/Src/Messaging.Core` |

---

## Step 1 — Define or Update an Entity

Create or modify a C# entity class inside the module's `Entities` folder.

```csharp
// Example: Workflow/Backend/Src/Workflow.Core/Entities/WorkflowRequest.cs
public class WorkflowRequest
{
    public int Id { get; set; }
    public int DayId { get; set; }
    public int RequestedVolume { get; set; }
    // ...
}
```

---

## Step 2 — Register the Entity in the DbContext

Add a `DbSet<T>` property to the module's DbContext and configure it in `OnModelCreating` if needed.

```csharp
// WorkflowDbContext.cs
public DbSet<WorkflowRequest> WorkflowRequests { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema(workflowCoreSettings?.Value.Db.DbSchema); // sets schema to "workflow"

    modelBuilder.Entity<WorkflowRequest>(entity =>
    {
        entity.HasDiscriminator(d => d.Type)
            .HasValue<SystemRequest>("System")
            .HasValue<TransportRequest>("Transport");
    });
}
```

Key conventions used across all DbContexts:
- `modelBuilder.HasDefaultSchema(...)` — scopes all tables to the module's PostgreSQL schema
- `.ToView("ViewName").HasKey(...)` — maps cross-schema views as read-only entities (no FK enforcement)
- `.HaveConversion<DbDateTimeConverter>()` — enforces UTC DateTime storage across all modules
- `AddInterceptors(auditFieldsInterceptor!)` — auto-populates audit fields on every `SaveChanges`

---

## Step 3 — Add a Migration

Run the following command **from the repository root**. Replace `<MigrationName>` with a descriptive PascalCase name.

### Config
```bash
cd Config/Backend/Src/Config.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context ConfigDbContext \
  -o Migrations
```

### Identity
```bash
cd Identity/Backend/Src/Identity.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context IdentityDbContext \
  -o Migrations
```

### Provider
```bash
cd Provider/Backend/Src/Provider.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context ProviderDbContext \
  -o Migrations
```

### Workflow
```bash
cd Workflow/Backend/Src/Workflow.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context WorkflowDbContext \
  -o Migrations
```

### Resource
```bash
cd Resource/Backend/Src/Resource.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context ResourceDbContext \
  -o Migrations
```

### Finance
```bash
cd Finance/Backend/Src/Finance.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context FinanceDbContext \
  -o Migrations
```

### Messaging
```bash
cd Messaging/Backend/Src/Messaging.Core
dotnet ef migrations add <MigrationName> \
  --project . \
  --startup-project ../../../../App/Backend/ \
  --context MessagingDbContext \
  -o Migrations
```

Each command creates two files in the module's `Migrations/` folder:
- `<Timestamp>_<MigrationName>.cs` — the `Up()` / `Down()` migration logic
- `<Timestamp>_<MigrationName>.Designer.cs` — EF snapshot metadata
- `*DbContextModelSnapshot.cs` — updated automatically to reflect current schema state

---

## Step 4 — Apply Migrations Locally

### Option A — Via the app (local dev only)

Set the flag in `appsettings.json` (or `.env`):

```json
"ShouldRunDbMigrationsFromApp": "true"
```

On startup, `App/Backend/Program.cs` resolves each DbContext from DI and calls `MigrateAsync()` in order:

```csharp
await configDbContext.Database.MigrateAsync();
await identityDbContext.Database.MigrateAsync();
await providerDbContext.Database.MigrateAsync();
await workflowDbContext.Database.MigrateAsync();
await resourceDbContext.Database.MigrateAsync();
await financeDbContext.Database.MigrateAsync();
await messagingDbContext.Database.MigrateAsync();
```

### Option B — Via EF CLI (per module)

```bash
dotnet ef database update \
  --project <module core project> \
  --startup-project App/Backend/ \
  --context <ModuleDbContext>
```

Example for Finance:
```bash
dotnet ef database update \
  --project Finance/Backend/Src/Finance.Core \
  --startup-project App/Backend/ \
  --context FinanceDbContext
```

---

## Step 5 — Configure appsettings.json

Each module reads its connection string and schema from `appsettings.json` under its own settings key. The `DbSchema` value must match the PostgreSQL schema name.

```json
{
  "ConfigSettings":    { "Core": { "Db": { "DbSchema": "config",    "ConnectionString": "..." } } },
  "IdentitySettings":  { "Core": { "Db": { "DbSchema": "identity",  "ConnectionString": "..." } } },
  "ProviderSettings":  { "Core": { "Db": { "DbSchema": "provider",  "ConnectionString": "..." } } },
  "WorkflowSettings":  { "Core": { "Db": { "DbSchema": "workflow",  "ConnectionString": "..." } } },
  "ResourceSettings":  { "Core": { "Db": { "DbSchema": "resource",  "ConnectionString": "..." } } },
  "FinanceSettings":   { "Core": { "Db": { "DbSchema": "finance",   "ConnectionString": "..." } } },
  "MessagingSettings": { "Core": { "Db": { "DbSchema": "messaging", "ConnectionString": "..." } } }
}
```

The `IDesignTimeDbContextFactory` in each DbContext file reads this config to provide EF CLI with a context at design time (when generating migrations), so `appsettings.json` must be present in the Core project directory or the App project.

---

## Step 6 — Build an EF Bundle (CI/CD)

EF Bundles are self-contained executables that apply migrations without the EF CLI or the full app runtime. They are the production migration strategy.

Build a bundle per module from the repo root:

```bash
dotnet ef migrations bundle \
  --project <module core project> \
  --startup-project App/Backend/App.csproj \
  --context <ModuleDbContext> \
  --force \
  --output <modulename>efbundle
```

Examples for all modules:

```bash
# Config
dotnet ef migrations bundle --project Config/Backend/Src/Config.Core --startup-project App/Backend/App.csproj --context ConfigDbContext --force --output configefbundle

# Identity
dotnet ef migrations bundle --project Identity/Backend/Src/Identity.Core --startup-project App/Backend/App.csproj --context IdentityDbContext --force --output identityefbundle

# Provider
dotnet ef migrations bundle --project Provider/Backend/Src/Provider.Core --startup-project App/Backend/App.csproj --context ProviderDbContext --force --output providerefbundle

# Workflow
dotnet ef migrations bundle --project Workflow/Backend/Src/Workflow.Core --startup-project App/Backend/App.csproj --context WorkflowDbContext --force --output workflowefbundle

# Resource
dotnet ef migrations bundle --project Resource/Backend/Src/Resource.Core --startup-project App/Backend/App.csproj --context ResourceDbContext --force --output resourceefbundle

# Finance
dotnet ef migrations bundle --project Finance/Backend/Src/Finance.Core --startup-project App/Backend/App.csproj --context FinanceDbContext --force --output financeefbundle

# Messaging
dotnet ef migrations bundle --project Messaging/Backend/Src/Messaging.Core --startup-project App/Backend/App.csproj --context MessagingDbContext --force --output messagingefbundle
```

The deployment script loops through `MODULE_SCHEMAS`, constructs a connection string per schema, and runs the matching bundle executable.

---

## Step 7 — Generate a Migration SQL Script (optional)

To review what SQL EF will execute before applying, generate a script:

```bash
dotnet ef migrations script \
  --project <module core project> \
  --startup-project App/Backend/ \
  --context <ModuleDbContext> \
  --idempotent \
  --output migration.sql
```

The `--idempotent` flag wraps each statement with existence checks so the script is safe to re-run.

---

## Migration Tracking

Each module tracks applied migrations in its own `__EFMigrationsHistory` table, scoped to its PostgreSQL schema (e.g. `workflow.__EFMigrationsHistory`). This keeps modules fully isolated — a failed Finance migration does not affect the Workflow schema.

---

## How DI Registration Works

Each module's `*CoreExtensions.cs` registers the DbContext into the ASP.NET DI container. Example pattern:

```csharp
// WorkflowCoreExtensions.cs
services.AddDbContext<WorkflowDbContext>(optionsBuilder =>
{
    optionsBuilder.UseNpgsql(
        workflowCoreSettings?.ConnectionString,
        sqlOptions =>
        {
            sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", workflowCoreSettings?.Db.DbSchema);
        });
});
```

`App/Backend/Program.cs` calls each module's extension method to wire everything up at startup.
