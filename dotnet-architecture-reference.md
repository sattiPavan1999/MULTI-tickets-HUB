# .NET Full-Stack Architecture Reference Guide

> **Purpose:** A technology- and domain-agnostic reference for building scalable, maintainable .NET 8 applications. Use this as a blueprint when starting a new module or greenfield project.

---

## Table of Contents

1. [Solution Overview](#1-solution-overview)
2. [Backend — Layered Architecture](#2-backend--layered-architecture)
   - 2.1 [Core Layer](#21-core-layer)
   - 2.2 [Endpoints Layer](#22-endpoints-layer)
   - 2.3 [Ingestor Layer](#23-ingestor-layer)
   - 2.4 [Background Workers Layer](#24-background-workers-layer)
3. [Frontend Architecture](#3-frontend-architecture)
4. [End-to-End Feature Development Flow](#4-end-to-end-feature-development-flow)
5. [API Design — GraphQL vs REST](#5-api-design--graphql-vs-rest)
6. [Patterns and Why They Are Used](#6-patterns-and-why-they-are-used)
7. [Dependency Injection and Configuration](#7-dependency-injection-and-configuration)
8. [Validation Strategy](#8-validation-strategy)
9. [Authorization Model](#9-authorization-model)
10. [Testing Strategy](#10-testing-strategy)
11. [Database and Migrations](#11-database-and-migrations)
12. [File Ingestion Pipeline](#12-file-ingestion-pipeline)
13. [Observability — Logging and Auditing](#13-observability--logging-and-auditing)
14. [Technology Stack Summary](#14-technology-stack-summary)
15. [Best Practices Checklist](#15-best-practices-checklist)

---

## 1. Solution Overview

The solution follows a **vertical-slice module** structure. Each business domain is self-contained in its own solution folder, with clear separation between:

- **Backend** — all C# server-side code
- **Frontend** — all TypeScript/React client code

```
<ModuleName>/
├── Backend/
│   ├── Src/
│   │   ├── <Module>.Core/             # Domain logic, entities, repositories, services
│   │   ├── <Module>.Endpoints/        # HTTP controllers, GraphQL queries, DTOs
│   │   ├── <Module>.Ingestor/         # File parsing and bulk data ingestion
│   │   └── <Module>.AzureFunctions/   # Async background workers (Durable Functions)
│   └── Tests/
│       ├── <Module>.Core.Tests/
│       ├── <Module>.Endpoints.Tests/
│       ├── <Module>.Ingestor.Tests/
│       └── <Module>.AzureFunctions.Tests/
└── Frontend/
    ├── src/
    │   ├── components/
    │   ├── services/
    │   ├── hooks/
    │   ├── types/
    │   ├── context/
    │   ├── utils/
    │   └── handlers/
    └── graphql/
```

### Why this structure?

| Decision | Reason |
|---|---|
| Module-per-domain | Strong team ownership; domain changes do not cross-cut the whole solution |
| `Src/` + `Tests/` separation | Keeps production assemblies distinct from test infrastructure |
| Shared cross-cutting concerns live in separate shared projects | Auth, notifications, time abstraction — avoid coupling modules to each other |

---

## 2. Backend — Layered Architecture

The backend follows a **clean/onion-inspired layered architecture** with four distinct project types.

```
┌───────────────────────────────────────────────────────────┐
│                    Endpoints / API Layer                    │
│          Controllers · GraphQL Queries · DTOs              │
├───────────────────────────────────────────────────────────┤
│                    Core / Business Layer                    │
│     Services · Repositories · Entities · Validators        │
├───────────────────────────────────────────────────────────┤
│               Infrastructure / Database Layer              │
│        EF Core DbContext · Migrations · Projections         │
├───────────────────────────────────────────────────────────┤
│              Ingestor / Background Workers Layer            │
│       File Parsers · Durable Functions · Activities         │
└───────────────────────────────────────────────────────────┘
```

---

### 2.1 Core Layer

**Project:** `<Module>.Core`

This is the heart of the application. It has zero knowledge of HTTP, GraphQL, or UI concerns.

#### Folder Structure

```
<Module>.Core/
├── Entities/           # EF Core domain models
├── Repository/
│   ├── Interfaces/     # IXxxRepository
│   └── Implementations/
├── Services/
│   ├── Interfaces/     # IXxxService
│   └── Implementations/
├── Business/           # Complex multi-step business workflows
│   ├── <Feature>/
│   │   ├── Validators/
│   │   ├── Processors/
│   │   └── Factories/
├── Validators/         # FluentValidation rules
├── Projections/        # Read-model projections for complex queries
├── Migrations/         # EF Core migration files
└── <Module>DbContext.cs
```

#### Entities

Domain entities are pure C# classes annotated for EF Core. They carry only data and minimal domain invariants.

```csharp
public class Customer
{
    public Guid Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime EnrolledAt { get; set; }

    // Navigation properties
    public ICollection<CustomerContract> Contracts { get; set; } = new List<CustomerContract>();
}
```

**Rules:**
- Entities do NOT contain business logic that belongs in services.
- Navigation properties are initialized to empty collections to prevent null reference errors.
- Use EF Core value converters for enums or custom types rather than storing raw strings.

#### Repository Interfaces

Every aggregate root gets its own interface derived from a generic base.

```csharp
public interface ICustomerRepository : IBaseRepository<Customer>
{
    Task<Customer?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Customer>> GetByStatusAsync(string status, CancellationToken ct = default);
}
```

The generic base provides standard CRUD:

```csharp
public interface IBaseRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    IQueryable<T> Query();
}
```

**Why interfaces?**
- Enables mocking in unit tests without hitting the database.
- Enforces the Dependency Inversion Principle — services depend on abstractions, not EF Core directly.

#### Service Interfaces and Implementations

Services are the primary entry points for business logic. They coordinate repositories, validators, and sub-services.

```csharp
public interface ICustomerService
{
    Task<CustomerDto> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task UpdateAsync(Guid id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
```

**Facade Pattern:** When a domain entity requires many distinct workflows (enrollment, billing, contract management), the top-level service delegates to specialized sub-services:

```
CustomerService  ─→  CustomerDataService        (queries / reads)
                 ─→  CustomerEnrollmentService   (enrollment workflow)
                 ─→  CustomerContractService     (contract lifecycle)
                 ─→  BilledUsageService          (billing operations)
```

This keeps each service focused and independently testable.

#### Business Workflow Folder

Complex multi-step processes that cannot be expressed cleanly as a single service method live in the `Business/` folder, organized by feature:

```
Business/
└── CustomerEnrollment/
    ├── Validators/
    │   └── CustomerEnrollmentValidator.cs
    ├── Processors/
    │   └── CustomerEnrollmentProcessor.cs
    └── Factories/
        └── CustomerEnrollmentFactory.cs
```

- **Validators** — check prerequisites before execution.
- **Processors** — execute the workflow steps.
- **Factories** — construct complex objects needed during processing.

---

### 2.2 Endpoints Layer

**Project:** `<Module>.Endpoints`

This layer is the only one that knows about HTTP, GraphQL, and request/response shapes.

#### Folder Structure

```
<Module>.Endpoints/
├── Controllers/          # REST endpoints (OData or standard)
├── GraphQL/
│   ├── Queries/          # HotChocolate Query classes
│   └── Types/            # GraphQL type definitions
├── DTOs/                 # Request/response models
├── Mapping/              # AutoMapper profiles
├── Strategies/           # Update strategy implementations
└── RequestTransformer/   # Authorization request transformation
```

#### Controllers

REST controllers handle simple CRUD and file upload endpoints that do not benefit from GraphQL's flexible querying.

```csharp
[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomerController(ICustomerService customerService)
        => _customerService = customerService;

    [HttpGet("{id:guid}")]
    [CasbinAuthorize("customer:read")]
    public async Task<ActionResult<CustomerDto>> Get(Guid id, CancellationToken ct)
    {
        var result = await _customerService.GetByIdAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    [CasbinAuthorize("customer:write")]
    public async Task<ActionResult<CustomerDto>> Create(CreateCustomerRequest request, CancellationToken ct)
    {
        var result = await _customerService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }
}
```

**Rules:**
- Controllers are thin — they validate route parameters and delegate immediately to services.
- No business logic lives in controllers.
- Return `ActionResult<T>` to make response types explicit.

#### DTOs

DTOs are the public contract. They are completely separate from domain entities.

```csharp
// Request DTO — what the API consumer sends
public record CreateCustomerRequest(
    string AccountNumber,
    string FirstName,
    string LastName,
    string ServiceAddress
);

// Response DTO — what the API returns
public record CustomerDto(
    Guid Id,
    string AccountNumber,
    string FullName,
    string Status,
    DateTime EnrolledAt
);
```

**Why DTOs instead of exposing entities directly?**
- Prevents over-posting attacks.
- Allows the API contract to evolve independently from the database schema.
- Enables projection optimizations (select only needed columns).

#### AutoMapper Profiles

Mapping between entities and DTOs is centralized in profile classes:

```csharp
public class CustomerMappingProfile : Profile
{
    public CustomerMappingProfile()
    {
        CreateMap<Customer, CustomerDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"));

        CreateMap<CreateCustomerRequest, Customer>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => "Pending"));
    }
}
```

#### GraphQL Queries (HotChocolate)

GraphQL is the primary query interface, enabling consumers to request exactly the fields they need with built-in filtering, sorting, and pagination.

```csharp
[ExtendObjectType("Query")]
public class CustomerQueries
{
    [UseDbContext(typeof(ModuleDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    [Authorize(Policy = "customer:read")]
    public IQueryable<Customer> GetCustomers([ScopedService] ModuleDbContext context)
        => context.Customers;
}
```

**Why `[UseProjection]`?**
EF Core only loads the columns that the GraphQL consumer requests. A query for `{ id, accountNumber }` generates `SELECT id, account_number FROM customers` — not `SELECT *`.

#### Strategy Pattern for Updates

When an entity can be updated via multiple workflows (each with different rules), use the Strategy pattern:

```csharp
public interface IEntityUpdateStrategy
{
    bool CanHandle(UpdateEntityRequest request);
    Task ExecuteAsync(Guid id, UpdateEntityRequest request, CancellationToken ct);
}

public class StatusUpdateStrategy : IEntityUpdateStrategy
{
    public bool CanHandle(UpdateEntityRequest request) => request.StatusChange is not null;

    public async Task ExecuteAsync(Guid id, UpdateEntityRequest request, CancellationToken ct)
    {
        // status-specific update logic
    }
}
```

A coordinator iterates registered strategies to find the appropriate one:

```csharp
public class UpdateCoordinator
{
    private readonly IEnumerable<IEntityUpdateStrategy> _strategies;

    public async Task UpdateAsync(Guid id, UpdateEntityRequest request, CancellationToken ct)
    {
        var strategy = _strategies.FirstOrDefault(s => s.CanHandle(request))
            ?? throw new InvalidOperationException("No strategy found for this request.");
        await strategy.ExecuteAsync(id, request, ct);
    }
}
```

---

### 2.3 Ingestor Layer

**Project:** `<Module>.Ingestor`

Handles bulk data import from external file feeds (CSV, fixed-width, JSON).

```
<Module>.Ingestor/
├── FileProcessors/
│   └── FileParsers/      # One parser per file type
├── FileLoggers/          # Per-record ingestion logging
├── Mapping/              # String-field to entity mapping
└── Exceptions/           # Custom parsing exceptions
```

#### File Parser Pattern

Each file type gets its own parser that validates structure and maps rows to domain objects:

```csharp
public class CustomerFileParser : IFileParser<CustomerFileRecord>
{
    public async Task<ParseResult<CustomerFileRecord>> ParseAsync(Stream stream, CancellationToken ct)
    {
        var records = new List<CustomerFileRecord>();
        var errors = new List<ParseError>();

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CsvConfiguration.Default);

        // Validate header row
        // Parse each row, capturing per-row errors without aborting
        // Validate trailer/footer count against header-declared count

        return new ParseResult<CustomerFileRecord>(records, errors);
    }
}
```

**Key behaviors:**
- Per-record errors are captured without aborting the whole file (partial success).
- Header/trailer counts are validated to detect truncated or duplicate deliveries.
- Every parse run writes to an ingestion log table for traceability.

---

### 2.4 Background Workers Layer

**Project:** `<Module>.AzureFunctions`

Long-running, scheduled, or fan-out operations run as Azure Durable Functions.

```
<Module>.AzureFunctions/
├── Triggers/        # TimerTrigger / BlobTrigger entry points
├── Orchestrators/   # Durable orchestration functions
├── Activities/      # Atomic, retriable work units
└── Services/        # Functions-specific service registrations
```

#### Orchestrator → Activity Pattern

```csharp
// Trigger starts the orchestration
[Function("MonthlyDataJob")]
public async Task Run([TimerTrigger("0 0 1 * *")] TimerInfo timer, [DurableClient] DurableTaskClient client)
    => await client.ScheduleNewOrchestrationInstanceAsync(nameof(MonthlyDataOrchestrator));

// Orchestrator coordinates activities
[Function(nameof(MonthlyDataOrchestrator))]
public async Task RunOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
{
    var files = await context.CallActivityAsync<List<string>>(nameof(ListPendingFilesActivity));

    var tasks = files.Select(file =>
        context.CallActivityAsync(nameof(ProcessFileActivity), file));

    await Task.WhenAll(tasks);          // fan-out / fan-in
    await context.CallActivityAsync(nameof(ArchiveFilesActivity), files);
}
```

**Why Durable Functions?**
- Built-in retry and checkpointing — safe to restart mid-processing.
- Fan-out/fan-in pattern for parallel file processing.
- State is persisted externally; the host can scale to zero between runs.

---

## 3. Frontend Architecture

**Stack:** React 18 · TypeScript · Apollo Client · React Hook Form · AG Grid · Styled Components · Vitest

```
Frontend/src/
├── components/           # Feature-scoped UI components
│   └── <FeatureName>/
│       ├── index.tsx
│       ├── <FeatureName>.tsx
│       ├── <FeatureName>.test.tsx
│       └── use<FeatureName>.ts   # co-located hook
├── services/             # Apollo GraphQL wrappers
├── hooks/                # Shared custom hooks
├── types/                # TypeScript type definitions
├── context/              # React Context providers
├── utils/                # Pure utility functions
├── config/               # Environment-based configuration
└── handlers/             # Event and error handlers
```

#### Component Design Principles

- **Container / Presentational split:** Container components own data-fetching; presentational components render props.
- **Co-located hooks:** Each feature component has a `use<FeatureName>.ts` hook alongside it that encapsulates query/mutation logic.
- **No direct Apollo calls in JSX:** All `useQuery` / `useMutation` calls live in hooks, keeping components pure UI.

```tsx
// use<Feature>.ts — data layer
export const useCustomerList = (filters: CustomerFilters) => {
    const { data, loading, error } = useQuery(GET_CUSTOMERS_QUERY, {
        variables: { filters },
        fetchPolicy: 'cache-and-network',
    });
    return { customers: data?.customers ?? [], loading, error };
};

// <Feature>.tsx — presentation layer
export const CustomerList: React.FC = () => {
    const { customers, loading, error } = useCustomerList(filters);
    if (loading) return <Spinner />;
    if (error) return <ErrorBoundary error={error} />;
    return <AgGridReact rowData={customers} columnDefs={columnDefs} />;
};
```

#### State Management Strategy

| State type | Tool |
|---|---|
| Server / remote data | Apollo Client cache |
| Form state | React Hook Form |
| Global UI state | React Context |
| Local component state | `useState` / `useReducer` |

Avoid introducing Redux or Zustand unless Apollo + Context is demonstrably insufficient. Context + Apollo covers the vast majority of real-world needs.

---

## 4. End-to-End Feature Development Flow

This section walks through adding a new feature from the database to the UI.

### Step 1 — Define the Entity (Core)

```csharp
// Core/Entities/Item.cs
public class Item
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTime CreatedAt { get; set; }
}
```

### Step 2 — Add EF Core Configuration & Migration (Core)

```csharp
// Inside ModuleDbContext.OnModelCreating
modelBuilder.Entity<Item>(entity =>
{
    entity.ToTable("Items", "module_schema");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
    entity.HasIndex(e => e.Name).IsUnique();
});
```

```bash
dotnet ef migrations add AddItemTable --project <Module>.Core
dotnet ef database update
```

### Step 3 — Define the Repository Interface (Core)

```csharp
public interface IItemRepository : IBaseRepository<Item>
{
    Task<Item?> GetByNameAsync(string name, CancellationToken ct = default);
}
```

### Step 4 — Implement the Repository (Core)

```csharp
public class ItemRepository : BaseRepository<Item>, IItemRepository
{
    public ItemRepository(ModuleDbContext context) : base(context) { }

    public Task<Item?> GetByNameAsync(string name, CancellationToken ct)
        => _context.Items.FirstOrDefaultAsync(i => i.Name == name, ct);
}
```

### Step 5 — Define and Implement the Service (Core)

```csharp
public interface IItemService
{
    Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken ct = default);
}

public class ItemService : IItemService
{
    private readonly IItemRepository _repo;
    private readonly IMapper _mapper;
    private readonly IValidator<CreateItemRequest> _validator;

    public ItemService(IItemRepository repo, IMapper mapper, IValidator<CreateItemRequest> validator)
    {
        _repo = repo; _mapper = mapper; _validator = validator;
    }

    public async Task<ItemDto> CreateAsync(CreateItemRequest request, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAsync(request, ct);
        var entity = _mapper.Map<Item>(request);
        await _repo.AddAsync(entity, ct);
        return _mapper.Map<ItemDto>(entity);
    }

    public async Task<IReadOnlyList<ItemDto>> GetAllAsync(CancellationToken ct)
    {
        var items = await _repo.Query().ToListAsync(ct);
        return _mapper.Map<IReadOnlyList<ItemDto>>(items);
    }
}
```

### Step 6 — Create DTOs and Mapping (Endpoints)

```csharp
public record CreateItemRequest(string Name);
public record ItemDto(Guid Id, string Name, string Status, DateTime CreatedAt);

public class ItemMappingProfile : Profile
{
    public ItemMappingProfile()
    {
        CreateMap<Item, ItemDto>();
        CreateMap<CreateItemRequest, Item>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(_ => DateTime.UtcNow));
    }
}
```

### Step 7 — Add Validation (Core)

```csharp
public class CreateItemRequestValidator : AbstractValidator<CreateItemRequest>
{
    public CreateItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters.");
    }
}
```

### Step 8 — Expose the Endpoint (Endpoints)

```csharp
[ApiController]
[Route("api/items")]
public class ItemController : ControllerBase
{
    private readonly IItemService _service;
    public ItemController(IItemService service) => _service = service;

    [HttpGet]
    public async Task<IReadOnlyList<ItemDto>> GetAll(CancellationToken ct) =>
        await _service.GetAllAsync(ct);

    [HttpPost]
    public async Task<ActionResult<ItemDto>> Create(CreateItemRequest request, CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetAll), result);
    }
}
```

### Step 9 — Add GraphQL Query (Endpoints)

```csharp
[ExtendObjectType("Query")]
public class ItemQueries
{
    [UseDbContext(typeof(ModuleDbContext))]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Item> GetItems([ScopedService] ModuleDbContext context)
        => context.Items;
}
```

### Step 10 — Register Everything (DI)

```csharp
// In CoreExtensions.cs or a DI registration file:
services.AddScoped<IItemRepository, ItemRepository>();
services.AddScoped<IItemService, ItemService>();
services.AddScoped<IValidator<CreateItemRequest>, CreateItemRequestValidator>();
services.AddAutoMapper(typeof(ItemMappingProfile));
```

### Step 11 — Write the GraphQL Query (Frontend)

```graphql
# graphql/queries/items.graphql
query GetItems($where: ItemFilterInput, $order: [ItemSortInput!]) {
  items(where: $where, order: $order) {
    id
    name
    status
    createdAt
  }
}
```

### Step 12 — Create the Hook (Frontend)

```ts
// components/ItemList/useItemList.ts
export const useItemList = (filters?: ItemFilters) => {
    const { data, loading, error } = useQuery(GET_ITEMS_QUERY, {
        variables: { where: filters },
    });
    return {
        items: data?.items ?? [],
        loading,
        error,
    };
};
```

### Step 13 — Create the Component (Frontend)

```tsx
// components/ItemList/ItemList.tsx
export const ItemList: React.FC = () => {
    const { items, loading, error } = useItemList();
    const columnDefs: ColDef[] = [
        { field: 'name', headerName: 'Name' },
        { field: 'status', headerName: 'Status' },
        { field: 'createdAt', headerName: 'Created' },
    ];
    if (loading) return <Spinner />;
    if (error) return <ErrorMessage error={error} />;
    return <AgGridReact rowData={items} columnDefs={columnDefs} />;
};
```

### Step 14 — Write Tests

See [Section 10 — Testing Strategy](#10-testing-strategy).

---

## 5. API Design — GraphQL vs REST

Both protocols coexist and each is used where it is strongest.

| Scenario | Use |
|---|---|
| Flexible tabular queries with filtering, sorting, pagination | GraphQL |
| File upload / download | REST |
| Simple CRUD endpoints with fixed shapes | REST |
| Cross-entity aggregation queries | GraphQL |
| Webhooks / callbacks | REST |
| Admin operations with complex authorization | Either, with Casbin policies on both |

### GraphQL Setup (HotChocolate)

```csharp
builder.Services
    .AddDbContextFactory<ModuleDbContext>(options => options.UseNpgsql(connectionString))
    .AddGraphQLServer()
    .AddQueryType(d => d.Name("Query"))
    .AddTypeExtension<ItemQueries>()
    .AddTypeExtension<CustomerQueries>()
    .AddProjections()
    .AddFiltering()
    .AddSorting()
    .AddAuthorization();
```

**Why `AddDbContextFactory` instead of `AddDbContext` for GraphQL?**
HotChocolate resolves fields in parallel. A scoped `DbContext` is not thread-safe for concurrent field resolution. `DbContextFactory` creates a short-lived context per field resolver, eliminating concurrency issues.

---

## 6. Patterns and Why They Are Used

### Repository Pattern

**Why:** Decouples business logic from EF Core. Services never import `Microsoft.EntityFrameworkCore` — they work against interfaces. This makes unit testing trivial (mock the interface) and makes it possible to swap the persistence provider.

### Service Facade Pattern

**Why:** A top-level service like `CustomerService` delegates to specialized sub-services. This keeps each sub-service small and independently testable, while callers have a single injection point.

### Strategy Pattern

**Why:** When the same operation (e.g., "update entity") has multiple valid implementations depending on the payload, strategies allow new update paths to be added without modifying existing code (Open/Closed Principle).

### Factory Pattern

**Why:** Object construction that involves business decisions (e.g., "which validator to use based on the status value") belongs in a factory, not in the service or controller.

### State Machine Pattern

**Why:** When an entity moves through a defined lifecycle (e.g., `Pending → InReview → Approved → Active`), encoding transitions as state classes prevents invalid transitions and centralizes transition logic.

```csharp
public abstract class EntityStatusBase
{
    public abstract string Name { get; }
    public abstract bool CanTransitionTo(EntityStatusBase target);
    public abstract EntityStatusBase TransitionTo(EntityStatusBase target);
}

public class PendingStatus : EntityStatusBase
{
    public override string Name => "Pending";
    public override bool CanTransitionTo(EntityStatusBase target)
        => target is InReviewStatus or CancelledStatus;
}
```

### Dependency Inversion (Interfaces Everywhere)

**Why:** Every injectable dependency is defined by its interface. Concrete types are registered in DI. This keeps the caller unaware of implementation details and makes swapping implementations (e.g., for testing or feature flags) cost-free.

---

## 7. Dependency Injection and Configuration

All DI registrations for the Core layer are grouped in an extension method:

```csharp
public static class CoreServiceExtensions
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration config)
    {
        // Database
        services.AddDbContext<ModuleDbContext>(options =>
            options.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        // Repositories
        services.AddScoped<IItemRepository, ItemRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();

        // Services
        services.AddScoped<IItemService, ItemService>();
        services.AddScoped<ICustomerService, CustomerService>();

        // Validators
        services.AddValidatorsFromAssemblyContaining<CreateItemRequestValidator>();

        // AutoMapper
        services.AddAutoMapper(typeof(ItemMappingProfile).Assembly);

        return services;
    }
}
```

This extension is called once in `Program.cs`:

```csharp
builder.Services.AddCoreServices(builder.Configuration);
```

**Why group registrations in extension methods?**
- `Program.cs` stays readable at a glance.
- Each layer owns its own registrations — the Endpoints layer does not register Core repositories.
- Easy to extract into a NuGet package if the module is shared.

---

## 8. Validation Strategy

Validation occurs at two levels.

### Level 1 — Structural Validation (FluentValidation)

Validates that the incoming request is well-formed before any business logic runs.

```csharp
public class UpdateContractRequestValidator : AbstractValidator<UpdateContractRequest>
{
    public UpdateContractRequestValidator()
    {
        RuleFor(x => x.StartDate).NotEmpty();
        RuleFor(x => x.EndDate)
            .NotEmpty()
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");
        RuleFor(x => x.RateCode)
            .NotEmpty()
            .MaximumLength(10);
    }
}
```

Register globally to get automatic 400 responses on validation failure:

```csharp
builder.Services
    .AddControllers()
    .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<UpdateContractRequestValidator>());
```

### Level 2 — Business Rule Validation (Service Layer)

Validates that the request is consistent with the current state of the domain (e.g., "you cannot enroll a customer who is already enrolled"):

```csharp
public async Task EnrollAsync(Guid customerId, EnrollmentRequest request, CancellationToken ct)
{
    var customer = await _repo.GetByIdAsync(customerId, ct)
        ?? throw new NotFoundException($"Customer {customerId} not found.");

    if (customer.Status == "Enrolled")
        throw new BusinessRuleViolationException("Customer is already enrolled.");

    // proceed with enrollment
}
```

**Rule:** Never mix Level 1 and Level 2 concerns. FluentValidation handles shape; services handle state.

---

## 9. Authorization Model

The project uses **Casbin** for attribute-based access control (ABAC), applied via a custom `[CasbinAuthorize]` attribute.

```csharp
[HttpDelete("{id:guid}")]
[CasbinAuthorize("customer:delete")]
public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
{
    await _service.DeleteAsync(id, ct);
    return NoContent();
}
```

A Casbin policy file defines which roles can perform which actions:

```
# policy.csv
p, admin,    customer:delete
p, manager,  customer:write
p, viewer,   customer:read
```

**Request Transformers** map incoming HTTP request context (user claims, route parameters) into Casbin subjects and objects before the policy check runs.

**Why Casbin over built-in ASP.NET Core policies?**
- Policy rules are data-driven (editable without redeployment).
- Supports complex role hierarchies and resource-scoped permissions.
- Decouples authorization logic from C# code.

---

## 10. Testing Strategy

### Philosophy

- **Unit tests** — test a single class in isolation. All dependencies are mocked.
- **Integration tests** — test the service+repository+database interaction using a real PostgreSQL instance via Testcontainers.
- **Endpoint tests** — test controllers and GraphQL queries against an in-process test server.

### Backend Test Projects

```
Tests/
├── <Module>.Core.Tests/          # Service and repository unit tests
├── <Module>.Endpoints.Tests/     # Controller and GraphQL integration tests
├── <Module>.Ingestor.Tests/      # File parser unit and integration tests
└── <Module>.AzureFunctions.Tests/# Orchestrator and activity tests
```

### Unit Test Structure (xUnit + Moq + FluentAssertions + Bogus)

```csharp
public class ItemServiceTests
{
    private readonly Mock<IItemRepository> _repoMock = new();
    private readonly Mock<IMapper> _mapperMock = new();
    private readonly IValidator<CreateItemRequest> _validator = new CreateItemRequestValidator();
    private readonly ItemService _sut;

    public ItemServiceTests()
        => _sut = new ItemService(_repoMock.Object, _mapperMock.Object, _validator);

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDto()
    {
        // Arrange
        var request = new Faker<CreateItemRequest>()
            .CustomInstantiator(f => new CreateItemRequest(f.Commerce.ProductName()))
            .Generate();

        var entity = new Item { Id = Guid.NewGuid(), Name = request.Name };
        var dto = new ItemDto(entity.Id, entity.Name, "Active", DateTime.UtcNow);

        _mapperMock.Setup(m => m.Map<Item>(request)).Returns(entity);
        _mapperMock.Setup(m => m.Map<ItemDto>(entity)).Returns(dto);

        // Act
        var result = await _sut.CreateAsync(request, CancellationToken.None);

        // Assert
        result.Should().BeEquivalentTo(dto);
        _repoMock.Verify(r => r.AddAsync(entity, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsValidationException()
    {
        var request = new CreateItemRequest(string.Empty);
        await _sut.Invoking(s => s.CreateAsync(request, CancellationToken.None))
            .Should().ThrowAsync<ValidationException>();
    }
}
```

**Conventions:**
- Method name: `<MethodUnderTest>_<Scenario>_<ExpectedOutcome>`
- Arrange/Act/Assert sections separated by blank lines with comments.
- Use `Bogus` (`Faker<T>`) for realistic test data rather than hardcoded strings.

### Integration Test Structure (Testcontainers)

```csharp
public class ItemRepositoryIntegrationTests : IAsyncLifetime
{
    private PostgreSqlContainer _postgres = null!;
    private ModuleDbContext _context = null!;

    public async Task InitializeAsync()
    {
        _postgres = new PostgreSqlBuilder().Build();
        await _postgres.StartAsync();

        var options = new DbContextOptionsBuilder<ModuleDbContext>()
            .UseNpgsql(_postgres.GetConnectionString())
            .Options;

        _context = new ModuleDbContext(options);
        await _context.Database.MigrateAsync();
    }

    [Fact]
    public async Task GetByNameAsync_ExistingItem_ReturnsItem()
    {
        var item = new Item { Id = Guid.NewGuid(), Name = "TestItem", Status = "Active" };
        _context.Items.Add(item);
        await _context.SaveChangesAsync();

        var repo = new ItemRepository(_context);
        var result = await repo.GetByNameAsync("TestItem");

        result.Should().NotBeNull();
        result!.Name.Should().Be("TestItem");
    }

    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
```

**Why Testcontainers?**
- Tests run against a real PostgreSQL engine, catching SQL generation issues that in-memory databases miss.
- Each test class gets a fresh container — no shared state.
- Works in CI/CD pipelines without external database dependencies.

### Frontend Test Structure (Vitest + React Testing Library)

```tsx
// components/ItemList/ItemList.test.tsx
import { render, screen, waitFor } from '@testing-library/react';
import { MockedProvider } from '@apollo/client/testing';
import { ItemList } from './ItemList';
import { GET_ITEMS_QUERY } from '../../services/itemQueries';

const mocks = [
    {
        request: { query: GET_ITEMS_QUERY, variables: {} },
        result: {
            data: {
                items: [
                    { id: '1', name: 'Widget A', status: 'Active', createdAt: '2024-01-01' },
                ],
            },
        },
    },
];

describe('ItemList', () => {
    it('renders items after loading', async () => {
        render(
            <MockedProvider mocks={mocks} addTypename={false}>
                <ItemList />
            </MockedProvider>
        );

        expect(screen.getByRole('progressbar')).toBeInTheDocument();

        await waitFor(() => {
            expect(screen.getByText('Widget A')).toBeInTheDocument();
        });
    });

    it('shows error state on query failure', async () => {
        const errorMocks = [{
            request: { query: GET_ITEMS_QUERY, variables: {} },
            error: new Error('Network error'),
        }];
        render(
            <MockedProvider mocks={errorMocks}>
                <ItemList />
            </MockedProvider>
        );
        await waitFor(() => {
            expect(screen.getByRole('alert')).toBeInTheDocument();
        });
    });
});
```

**Frontend test conventions:**
- Use `MockedProvider` from Apollo to mock GraphQL without a real server.
- Test user-visible behavior, not implementation details.
- Assert on accessible roles and text, not internal state or class names.
- Co-locate test files next to the component (`ItemList.test.tsx` beside `ItemList.tsx`).

### Test Coverage Targets

| Layer | Target |
|---|---|
| Service unit tests | ≥ 85% branch coverage |
| Repository integration tests | All public methods covered |
| Controller / GraphQL tests | All happy paths + common error paths |
| Frontend component tests | All user-visible states (loading, error, data) |
| File parser tests | Valid file, malformed rows, truncated file, duplicate file |

---

## 11. Database and Migrations

### EF Core Setup

The `DbContext` lives in the Core project. It uses a dedicated database schema to namespace all tables for the module:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.HasDefaultSchema("module_schema");
    // Entity configurations...
}
```

**Why a dedicated schema?**
Multiple modules share the same PostgreSQL database cluster. Schema namespacing prevents table name collisions and allows schema-scoped permissions.

### Migration Conventions

```bash
# Add a migration
dotnet ef migrations add <DescriptiveName> --project <Module>.Core --startup-project <Module>.Endpoints

# Apply migrations
dotnet ef database update --project <Module>.Core --startup-project <Module>.Endpoints
```

**Naming convention for migrations:**
- `AddCustomerTable`
- `AddIndexOnCustomerAccountNumber`
- `AddEnrolledAtToCustomer`
- `RenameContractEndDateColumn`

**Rules:**
- Never edit an already-applied migration. Add a new one.
- Review generated SQL (`dotnet ef migrations script`) before applying to production.
- Migrations that add NOT NULL columns should include a `defaultValue` to handle existing rows.

### Audit Trail

Entity change tracking is enabled via `Audit.EntityFramework`:

```csharp
services.AddAuditEntityFramework(config =>
{
    config.Mode = AuditOptionMode.OptIn;
    config.IncludeEntityObjects = false;
    config.AuditEventType = "{context}:{table}";
});
```

All `INSERT`, `UPDATE`, and `DELETE` operations are written to an audit log table automatically.

---

## 12. File Ingestion Pipeline

The pipeline follows a consistent pattern for processing externally delivered data files.

```
Blob Storage
     │
     ▼
Azure Function Trigger
     │
     ▼
Orchestrator (coordinates fan-out)
     │
     ├──▶ ListPendingFilesActivity
     │
     ├──▶ ParseFileActivity (per file)
     │        │
     │        ├──▶ FileParser (CsvHelper)
     │        ├──▶ Row Validator (FluentValidation)
     │        └──▶ IngestionLog Writer
     │
     ├──▶ PersistDataActivity (bulk insert)
     │
     └──▶ ArchiveFileActivity
```

### Error Handling in Ingestion

Individual row errors do NOT abort the file. Each failure is recorded in the ingestion log:

```csharp
foreach (var row in parsedRows)
{
    var validationResult = await _validator.ValidateAsync(row, ct);
    if (!validationResult.IsValid)
    {
        await _logWriter.WriteErrorAsync(fileName, row.LineNumber, validationResult.Errors, ct);
        continue;
    }
    processedRows.Add(_mapper.Map<EntityType>(row));
}
```

This produces a per-file error report visible in the UI, allowing operations teams to resubmit corrected files.

---

## 13. Observability — Logging and Auditing

### Structured Logging with Serilog

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .WriteTo.Console()
    .WriteTo.Seq(builder.Configuration["Seq:ServerUrl"]!)
    .WriteTo.ApplicationInsights(telemetryConfiguration, TelemetryConverter.Traces)
    .CreateLogger();
```

**Log level conventions:**
- `Information` — normal workflow steps (file received, record processed, status changed)
- `Warning` — unexpected but recoverable situations (duplicate file, validation failure)
- `Error` — exceptions that affect a single request
- `Critical` — exceptions that affect the entire service

### Correlation IDs

Attach a correlation ID to every request for end-to-end tracing across services:

```csharp
app.Use(async (ctx, next) =>
{
    var correlationId = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();
    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        ctx.Response.Headers["X-Correlation-Id"] = correlationId;
        await next();
    }
});
```

---

## 14. Technology Stack Summary

### Backend

| Category | Technology | Version |
|---|---|---|
| Runtime | .NET | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL (Npgsql) | 8.0 |
| GraphQL | HotChocolate | 14.x |
| REST query | Microsoft.AspNetCore.OData | 8.x |
| Validation | FluentValidation | 11.x |
| Object mapping | AutoMapper | 13.x |
| CSV parsing | CsvHelper | 32.x |
| Logging | Serilog | 8.x |
| Authorization | Casbin | 1.x |
| Auditing | Audit.EntityFramework | 27.x |
| Serverless | Azure Durable Functions | 1.x |
| Unit testing | xUnit + Moq + FluentAssertions | Latest |
| Fake data | Bogus | 35.x |
| Integration tests | Testcontainers (PostgreSQL) | 3.x |

### Frontend

| Category | Technology | Version |
|---|---|---|
| UI framework | React | 18.x |
| Language | TypeScript | 5.x |
| App framework | Next.js | 14.x |
| GraphQL client | Apollo Client | 3.x |
| Forms | React Hook Form | 7.x |
| Data grid | AG Grid | 30.x |
| Styling | Styled Components | 6.x |
| Unit tests | Vitest | 2.x |
| Test utilities | React Testing Library | 14.x |

---

## 15. Best Practices Checklist

Use this checklist when starting or reviewing a module.

### Architecture

- [ ] Domain logic lives only in the Core layer — no business logic in controllers or GraphQL resolvers
- [ ] Every injectable dependency is expressed as an interface
- [ ] Services use the Facade pattern when a domain has multiple sub-workflows
- [ ] State-machine pattern is used for entities with lifecycle transitions
- [ ] Strategy pattern is used when multiple update paths exist for the same entity
- [ ] Factories encapsulate conditional object construction

### Database

- [ ] All entities use a dedicated schema (`HasDefaultSchema`)
- [ ] Unique constraints are defined at the database level, not just in application code
- [ ] All migrations have descriptive names
- [ ] No `SELECT *` in projections — use `[UseProjection]` or explicit column lists
- [ ] Audit trail is enabled on all mutable aggregate roots
- [ ] Bulk operations use `BulkInsert` / `BulkSaveChanges` for high-volume data

### API Design

- [ ] GraphQL is used for flexible querying; REST for file upload/download and simple CRUD
- [ ] DTOs are separate from domain entities
- [ ] AutoMapper profiles are registered per layer
- [ ] All endpoints have authorization attributes
- [ ] Validation errors return 400 with structured error bodies

### Testing

- [ ] Unit tests follow `<Method>_<Scenario>_<ExpectedOutcome>` naming
- [ ] Integration tests use Testcontainers (real database)
- [ ] Test data is generated with Bogus, not hardcoded strings
- [ ] All services have unit tests for at least happy path + validation failure
- [ ] File parsers have tests for: valid file, malformed row, truncated file, duplicate
- [ ] Frontend components have tests for: loading state, error state, populated state

### Observability

- [ ] Serilog is configured with structured properties (not string interpolation)
- [ ] Correlation IDs are propagated through all HTTP requests
- [ ] Application Insights / Seq is configured
- [ ] All Azure Functions have activity-level logging

### Security

- [ ] Casbin policies are used, not hardcoded role strings
- [ ] No secrets in `appsettings.json` — use Azure Key Vault or environment variables
- [ ] Input validation is applied at all system boundaries
- [ ] No sensitive data is logged (PII, account numbers)

### Frontend

- [ ] All `useQuery` / `useMutation` calls live in hooks, not JSX
- [ ] Components receive data via props; data fetching is in the hook layer
- [ ] Error boundaries wrap all data-fetching components
- [ ] GraphQL queries request only the fields the component uses
- [ ] Forms use React Hook Form — no raw `useState` for form fields

---

*This document was generated from a real-world .NET 8 production module as a technology- and domain-agnostic reference. Update the version numbers in Section 14 when adopting newer library versions.*
