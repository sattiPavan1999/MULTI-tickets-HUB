# Repository Tests Setup Guide

A step-by-step guide to setting up and running repository/integration tests in a .NET project using **Testcontainers** and **PostgreSQL**.

---

## How it works

Integration tests use **Testcontainers** to spin up a real PostgreSQL Docker container automatically per test class. You never manage a database manually. Each test class:

1. Starts a fresh PostgreSQL container.
2. Runs EF Core migrations to create the schema.
3. Executes the tests against the real database.
4. Stops and destroys the container when the test class finishes.

**What you need on your machine:** just a running Docker daemon (via Colima or Docker Desktop).

---

## Prerequisites

### 1. .NET 8 SDK

```bash
dotnet --version   # must be 8.x
```

Download: https://dotnet.microsoft.com/download/dotnet/8.0

---

### 2. Docker runtime

Testcontainers needs a Docker daemon running locally.

#### Option A — Colima (lightweight, recommended on macOS)

**Install:**
```bash
brew install colima
```

**Start:**
```bash
colima start
```

**Verify:**
```bash
docker info | grep "Server Version"
# Expected: Server Version: 27.x.x
```

#### Option B — Docker Desktop

Just open the Docker Desktop app and wait for the whale icon to show "Docker Desktop is running".

**Verify (same command):**
```bash
docker info | grep "Server Version"
```

---

### 3. Required NuGet packages

Add these to your test project's `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.8.0" />
  <PackageReference Include="xunit" Version="2.5.3" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
  <PackageReference Include="Testcontainers" Version="3.8.0" />
  <PackageReference Include="Testcontainers.PostgreSql" Version="3.8.0" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.8">
    <PrivateAssets>all</PrivateAssets>
  </PackageReference>
  <PackageReference Include="Moq" Version="4.20.70" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

---

## Step-by-step setup

### Step 1 — Start Docker

**Colima:**
```bash
colima start
```

**Docker Desktop:** open the app.

Confirm Docker is reachable:
```bash
docker ps   # should list running containers (or be empty), not throw an error
```

---

### Step 2 — Create `appsettings.Test.json`

The configuration loader reads `appsettings.Test.json` from the working directory at runtime. This file is typically **not committed** to git (add it to `.gitignore`). Create it in your test project directory.

```json
{
  "DatabaseSettings": {
    "DbSchema": "your_schema_name",
    "ConnectionString": "ConnectionString_Placeholder"
  }
}
```

> **Note:** The `ConnectionString` value here is a placeholder. Testcontainers generates the real connection string at runtime and injects it into your `DbContext`. The schema name is what matters.

Load it in your test project with:

```csharp
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.Test.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables()
    .Build();
```

---

### Step 3 — Create a base integration test class

This class handles container lifecycle (start → migrate → test → stop) for every test class that inherits it.

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;
using Xunit;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres;

    protected YourDbContext DbContext { get; private set; }
    protected string ConnectionString { get; private set; }

    protected IntegrationTestBase()
    {
        _postgres = new PostgreSqlBuilder()
            .WithLogger(LoggerFactory.Create(b => b.AddConsole()).CreateLogger("postgres"))
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();
        await _postgres.WaitForPort();  // see workaround below

        ConnectionString = _postgres.GetConnectionString();

        var options = new DbContextOptionsBuilder<YourDbContext>()
            .UseNpgsql(ConnectionString, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "your_schema"))
            .Options;

        DbContext = new YourDbContext(options);
        await DbContext.Database.MigrateAsync();
    }

    public async Task DisposeAsync()
    {
        await _postgres.StopAsync();
    }
}
```

---

### Step 4 — Port readiness workaround

Testcontainers sometimes reports a container as ready before PostgreSQL is actually accepting TCP connections. Add this helper to avoid flaky test starts:

```csharp
using DotNet.Testcontainers.Containers;
using System.Net;
using System.Net.Sockets;
using Testcontainers.PostgreSql;

public static class TestContainerExtensions
{
    public static Task<bool> WaitForPort(
        this PostgreSqlContainer container,
        TimeSpan? maxWait = null)
    {
        return WaitForPort(container, PostgreSqlBuilder.PostgreSqlPort,
            maxWait ?? TimeSpan.FromSeconds(10));
    }

    private static async Task<bool> WaitForPort(
        DockerContainer container, int port, TimeSpan maxWait)
    {
        var ips = await Dns.GetHostAddressesAsync(container.Hostname);
        var ip = ips.First(i => i.AddressFamily == AddressFamily.InterNetwork);
        int mapped = container.GetMappedPublicPort(port);

        using var cts = new CancellationTokenSource(maxWait);
        using var tcp = new TcpClient();

        while (!cts.IsCancellationRequested)
        {
            try
            {
                await tcp.ConnectAsync(ip, mapped, cts.Token);
                return true;
            }
            catch (SocketException) { }

            await Task.Delay(50, cts.Token);
        }

        return false;
    }
}
```

---

### Step 5 — Write a repository test

```csharp
public class ProductRepositoryTests : IntegrationTestBase
{
    [Fact]
    public async Task Save_and_retrieve_product()
    {
        var repo = new ProductRepository(DbContext);

        var product = new Product { Name = "Widget", Price = 9.99m };
        await repo.AddAsync(product);
        await DbContext.SaveChangesAsync();

        var result = await repo.GetByIdAsync(product.Id);

        result.Should().NotBeNull();
        result!.Name.Should().Be("Widget");
    }
}
```

---

### Step 6 — Restore, build, and run

```bash
# restore dependencies
dotnet restore

# build (fix any errors before running tests)
dotnet build

# run all tests
dotnet test

# run only integration tests (if you use a Category attribute)
dotnet test --filter "Category=IntegrationTest"

# run a single test class
dotnet test --filter "FullyQualifiedName~ProductRepositoryTests"

# verbose output (useful while debugging container startup)
dotnet test --logger "console;verbosity=detailed"
```

---

## PostgreSQL log verbosity

By default container logs are minimal. To see all SQL queries:

```bash
INT_TEST_POSTGRES_LOG_LEVEL=Debug dotnet test
```

Valid values: `Trace`, `Debug`, `Information`, `Warning`, `Error`, `Critical`.

---

## Common errors and fixes

### `Cannot connect to the Docker daemon`

Docker is not running.

```bash
colima start        # Colima users
# or open Docker Desktop
docker info         # verify
```

---

### `Error pulling image postgres`

No internet access, or Docker Hub is unreachable. Pull manually to see the real error:

```bash
docker pull postgres:latest
```

---

### `NullReferenceException` reading configuration

`appsettings.Test.json` is missing or a required key is absent. Confirm the file exists in the build output directory:

```bash
ls bin/Debug/net8.0/appsettings.Test.json
```

If it is missing, add this to your `.csproj`:

```xml
<ItemGroup>
  <Content Include="appsettings.Test.json">
    <CopyToOutputDirectory>Always</CopyToOutputDirectory>
  </Content>
</ItemGroup>
```

---

### `WaitForPort` timeout

PostgreSQL took longer than 10 seconds to accept connections. Pass a longer timeout:

```csharp
await _postgres.WaitForPort(TimeSpan.FromSeconds(30));
```

---

### Migrations fail — schema does not exist

The schema name in your `MigrationsHistoryTable(...)` call does not match the schema your migrations create. Make sure both use the same value.

---

### Tests pass locally but fail in CI

The CI runner has no Docker socket. In your CI pipeline (GitHub Actions example):

```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.x'
      - run: dotnet test
    # Docker is available by default on ubuntu-latest — no extra setup needed
```

For self-hosted runners, ensure Docker is installed and the runner user is in the `docker` group.

---

## Quick reference

| Command | Purpose |
|---|---|
| `colima start` | Start Docker runtime (macOS) |
| `docker ps` | Verify Docker is reachable |
| `dotnet restore` | Install NuGet packages |
| `dotnet build` | Compile before testing |
| `dotnet test` | Run all tests |
| `dotnet test --filter "Category=IntegrationTest"` | Run integration tests only |
| `INT_TEST_POSTGRES_LOG_LEVEL=Debug dotnet test` | Enable SQL query logging |
