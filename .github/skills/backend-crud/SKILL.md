---
name: backend-crud
description: Step-by-step instructions for creating a complete backend CRUD in the AppProject .NET template, including DTOs, database entities, EF Core configuration, Mapster mapping, services, summary services, and controllers. Use when the user wants to create a new entity/CRUD on the backend/API side, or when they need to add DTOs, services, or controllers for a new entity.
metadata:
  author: appproject
  version: "1.0"
---

# Backend CRUD Creation

Follow these steps in order to create a complete backend CRUD for a new entity. Use the **General** module (Country, State, City) as the reference implementation.

## Step 1: Identify the Module

Determine which module the entity belongs to. If the module already exists (e.g., `General`), add files to the existing projects. If a new module is needed, see the `new-module` skill first.

## Step 2: Create the Entity DTO

**Location:** `src/AppProject.Core.Models.<Module>/`

Entity DTOs represent the table fields. They MUST:
- Inherit from `IEntity`
- Expose `RowVersion` for optimistic concurrency
- Use DataAnnotations for validation (`[Required]`, `[MaxLength]`, etc.)
- Use `[RequiredGuid]` for required foreign key GUID fields

### Reference Pattern (Country — simple entity):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;

namespace AppProject.Core.Models.<Module>;

public class <EntityName> : IEntity
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    // Add other properties here...

    public byte[]? RowVersion { get; set; }
}
```

### Reference Pattern (State — entity with FK):

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using AppProject.Models;
using AppProject.Models.CustomValidators;

namespace AppProject.Core.Models.<Module>;

public class <EntityName> : IEntity
{
    public Guid? Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [RequiredGuid]
    public Guid ParentEntityId { get; set; }

    public byte[]? RowVersion { get; set; }
}
```

### Entity with Nested Children (City pattern):

When an entity has child collections managed through the parent form:
```csharp
[ValidateCollection]
public ICollection<CreateOrUpdateRequest<ChildEntity>>? ChangedChildRequests { get; set; }

public ICollection<DeleteRequest<Guid>>? DeletedChildRequests { get; set; }
```

## Step 3: Create the Summary DTO

**Location:** `src/AppProject.Core.Models/<Module>/` (shared) or `src/AppProject.Core.Models.<Module>/` (module-specific)

Summary DTOs are for grids, combos, and read queries. They MUST:
- Inherit from `ISummary`
- Have NO DataAnnotations
- Contain only display fields

```csharp
using System;
using AppProject.Models;

namespace AppProject.Core.Models.<Module>;

public class <EntityName>Summary : ISummary
{
    public Guid Id { get; set; }

    public string Name { get; set; } = default!;

    // Add aggregated names for related data (e.g., ParentEntityName)
}
```

## Step 4: Create SearchRequest (ONLY if extra filters exist)

**Only create a derived SearchRequest when you need additional filters** beyond `SearchText` and `Take`. If no extra filters are needed, use the base `SearchRequest` directly.

```csharp
using AppProject.Models;

namespace AppProject.Core.Models.<Module>;

public class <EntityName>SummarySearchRequest : SearchRequest
{
    public Guid? ParentEntityId { get; set; }
}
```

## Step 5: Create the Database Entity

**Location:** `src/AppProject.Core.Infrastructure.Database/Entities/<Module>/`

Follow the `Tb[Name]` naming pattern:

```csharp
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AppProject.Core.Infrastructure.Database.Entities.<Module>;

[Table("<PluralName>")]
public class Tb<EntityName> : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    // Foreign key example:
    // public Guid ParentEntityId { get; set; }
    // [ForeignKey(nameof(ParentEntityId))]
    // public TbParentEntity ParentEntity { get; set; } = default!;

    // Navigation collection example:
    // public ICollection<TbChildEntity> Children { get; set; } = new List<TbChildEntity>();
}
```

**Rules:**
- Use `[Table("PluralName")]` with plural table names
- Use `[Key]` on the primary key
- Use `[ForeignKey]` with navigation properties
- Use `[MaxLength]` on text columns
- Add `ICollection<T>` for navigation collections

## Step 6: Create EntityTypeConfiguration

**Location:** `src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/<Module>/`

```csharp
using AppProject.Core.Infrastructure.Database.Entities.<Module>;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AppProject.Core.Infrastructure.Database.EntityTypeConfiguration.<Module>;

public class Tb<EntityName>Configuration : IEntityTypeConfiguration<Tb<EntityName>>
{
    public void Configure(EntityTypeBuilder<Tb<EntityName>> builder)
    {
        builder.HasIndex(x => x.Name).IsUnique();
        // Add additional indexes as needed
    }
}
```

**Note:** No manual registration needed. `ApplicationDbContext.OnModelCreating()` auto-discovers all `IEntityTypeConfiguration<T>` via assembly scanning.

## Step 7: Add DbSet to ApplicationDbContext

**Location:** `src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs`

Add a new `DbSet<T>` with a plural name:

```csharp
public DbSet<Tb<EntityName>> <PluralName> { get; set; } = default!;
```

## Step 8: Create Mapster Configuration (ONLY if needed)

**Location:** `src/AppProject.Core.Infrastructure.Database/Mapper/<Module>/`

Only create when property names between entity and DTO differ (e.g., mapping navigation property names):

```csharp
using AppProject.Core.Infrastructure.Database.Entities.<Module>;
using AppProject.Core.Models.<Module>;
using Mapster;

namespace AppProject.Core.Infrastructure.Database.Mapper.<Module>;

public class <EntityName>SummaryMapsterConfig : IRegisterMapsterConfig
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Tb<EntityName>, <EntityName>Summary>()
            .Map(dest => dest.ParentEntityName, src => src.ParentEntity.Name);
    }
}
```

## Step 9: Create the Service Interface

**Location:** `src/AppProject.Core.Services.<Module>/`

```csharp
using System;
using AppProject.Core.Models.<Module>;
using AppProject.Models;

namespace AppProject.Core.Services.<Module>;

public interface I<EntityName>Service
    : ITransientService,
    IGetEntity<GetByIdRequest<Guid>, EntityResponse<<EntityName>>>,
    IPostEntity<CreateOrUpdateRequest<<EntityName>>, KeyResponse<Guid>>,
    IPutEntity<CreateOrUpdateRequest<<EntityName>>, KeyResponse<Guid>>,
    IDeleteEntity<DeleteRequest<Guid>, EmptyResponse>
{
}
```

## Step 10: Create the Service Implementation

**Location:** `src/AppProject.Core.Services.<Module>/`

Every service method MUST:
1. Validate permissions with `IPermissionService.ValidateCurrentUserPermissionAsync`
2. Run business validations (duplicate check, etc.)
3. Use `IDatabaseRepository` for data access
4. Map DTOs ↔ entities via `Mapster` (`Adapt`)
5. Throw `AppException` with appropriate `ExceptionCode`

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.<Module>;
using AppProject.Core.Models.<Module>;
using AppProject.Core.Services.Auth;
using AppProject.Exceptions;
using AppProject.Models;
using AppProject.Models.Auth;
using Mapster;

namespace AppProject.Core.Services.<Module>;

public class <EntityName>Service(
    IDatabaseRepository databaseRepository,
    IPermissionService permissionService)
    : BaseService, I<EntityName>Service
{
    public async Task<EntityResponse<<EntityName>>> GetEntityAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.<PermissionName>, cancellationToken: cancellationToken);

        var entity = await databaseRepository.GetFirstOrDefaultAsync<Tb<EntityName>, <EntityName>>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (entity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new EntityResponse<<EntityName>> { Entity = entity };
    }

    public async Task<KeyResponse<Guid>> PostEntityAsync(CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.<PermissionName>, cancellationToken: cancellationToken);
        await this.Validate<EntityName>Async(request.Entity, cancellationToken);

        var tbEntity = request.Entity.Adapt<Tb<EntityName>>();
        await databaseRepository.InsertAndSaveAsync(tbEntity, cancellationToken);

        return new KeyResponse<Guid> { Id = tbEntity.Id };
    }

    public async Task<KeyResponse<Guid>> PutEntityAsync(CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.<PermissionName>, cancellationToken: cancellationToken);
        await this.Validate<EntityName>Async(request.Entity, cancellationToken);

        var tbEntity = await databaseRepository.GetFirstOrDefaultAsync<Tb<EntityName>>(
            query => query.Where(x => x.Id == request.Entity.Id),
            cancellationToken);

        if (tbEntity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        request.Entity.Adapt(tbEntity);
        await databaseRepository.UpdateAndSaveAsync(tbEntity, cancellationToken);

        return new KeyResponse<Guid> { Id = tbEntity.Id };
    }

    public async Task<EmptyResponse> DeleteEntityAsync(DeleteRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        await permissionService.ValidateCurrentUserPermissionAsync(PermissionType.<PermissionName>, cancellationToken: cancellationToken);

        var tbEntity = await databaseRepository.GetFirstOrDefaultAsync<Tb<EntityName>>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (tbEntity == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        await databaseRepository.DeleteAndSaveAsync(tbEntity, cancellationToken);
        return new EmptyResponse();
    }

    private async Task Validate<EntityName>Async(<EntityName> entity, CancellationToken cancellationToken = default)
    {
        if (await databaseRepository.HasAnyAsync<Tb<EntityName>>(
            query => query.Where(x => x.Name == entity.Name && x.Id != entity.Id),
            cancellationToken))
        {
            throw new AppException(ExceptionCode.<Module>_<EntityName>_DuplicateName);
        }
    }
}
```

## Step 11: Create the Summary Service Interface

**Location:** `src/AppProject.Core.Services/<Module>/` (shared)

```csharp
using System;
using AppProject.Core.Models.<Module>;
using AppProject.Models;

namespace AppProject.Core.Services.<Module>;

// Use SearchRequest directly if no extra filters needed
public interface I<EntityName>SummaryService
    : ITransientService,
    IGetSummaries<SearchRequest, SummariesResponse<<EntityName>Summary>>,
    IGetSummary<GetByIdRequest<Guid>, SummaryResponse<<EntityName>Summary>>
{
}
```

If extra filters exist, use the custom SearchRequest:
```csharp
IGetSummaries<<EntityName>SummarySearchRequest, SummariesResponse<<EntityName>Summary>>,
```

## Step 12: Create the Summary Service Implementation

**Location:** `src/AppProject.Core.Services/<Module>/`

```csharp
using System;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.<Module>;
using AppProject.Core.Models.<Module>;
using AppProject.Exceptions;
using AppProject.Models;

namespace AppProject.Core.Services.<Module>;

public class <EntityName>SummaryService(
    IDatabaseRepository databaseRepository)
    : BaseService, I<EntityName>SummaryService
{
    public async Task<SummariesResponse<<EntityName>Summary>> GetSummariesAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        var searchText = request.SearchText?.Trim();

        var summaries = await databaseRepository.GetByConditionAsync<Tb<EntityName>, <EntityName>Summary>(
            query =>
            {
                if (!string.IsNullOrWhiteSpace(searchText))
                {
                    query = query.Where(x =>
                        x.Id.ToString().Contains(searchText) || x.Name.Contains(searchText));
                }

                query = query.OrderBy(x => x.Name);

                if (request.Take.HasValue)
                {
                    query = query.Take(request.Take.Value);
                }

                return query;
            },
            cancellationToken);

        return new SummariesResponse<<EntityName>Summary> { Summaries = summaries };
    }

    public async Task<SummaryResponse<<EntityName>Summary>> GetSummaryAsync(GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
    {
        var summary = await databaseRepository.GetFirstOrDefaultAsync<Tb<EntityName>, <EntityName>Summary>(
            query => query.Where(x => x.Id == request.Id),
            cancellationToken);

        if (summary == null)
        {
            throw new AppException(ExceptionCode.EntityNotFound);
        }

        return new SummaryResponse<<EntityName>Summary> { Summary = summary };
    }
}
```

## Step 13: Create the CRUD Controller

**Location:** `src/AppProject.Core.Controllers.<Module>/`

```csharp
using AppProject.Core.Models.<Module>;
using AppProject.Core.Services.<Module>;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.<Module>
{
    [Route("api/<module_lowercase>/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class <EntityName>Controller(I<EntityName>Service service)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await service.GetEntityAsync(request, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken)
        {
            return this.Ok(await service.PostEntityAsync(request, cancellationToken));
        }

        [HttpPut]
        public async Task<IActionResult> PutAsync([FromBody] CreateOrUpdateRequest<<EntityName>> request, CancellationToken cancellationToken)
        {
            return this.Ok(await service.PutEntityAsync(request, cancellationToken));
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteAsync([FromQuery] DeleteRequest<Guid> request, CancellationToken cancellationToken)
        {
            return this.Ok(await service.DeleteEntityAsync(request, cancellationToken));
        }
    }
}
```

## Step 14: Create the Summary Controller

**Location:** `src/AppProject.Core.Controllers.<Module>/`

```csharp
using AppProject.Core.Services.<Module>;
using AppProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppProject.Core.Controllers.<Module>
{
    [Route("api/<module_lowercase>/[controller]/[action]")]
    [ApiController]
    [Authorize]
    public class <EntityName>SummaryController(I<EntityName>SummaryService summaryService)
        : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetSummariesAsync([FromQuery] SearchRequest request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await summaryService.GetSummariesAsync(request, cancellationToken));
        }

        [HttpGet]
        public async Task<IActionResult> GetSummaryAsync([FromQuery] GetByIdRequest<Guid> request, CancellationToken cancellationToken = default)
        {
            return this.Ok(await summaryService.GetSummaryAsync(request, cancellationToken));
        }
    }
}
```

## Step 15: Add ExceptionCode

**Location:** `src/AppProject.Exceptions/ExceptionCode.cs`

Add a new entry following the pattern `<Module>_<EntityName>_<ValidationName>`:

```csharp
<Module>_<EntityName>_DuplicateName,
```

## Step 16: Add Resource Keys

Add translated messages for the new ExceptionCode to all three `.resx` files:
- `src/AppProject.Resources/Resource.resx` (English)
- `src/AppProject.Resources/Resource.pt-BR.resx` (Portuguese)
- `src/AppProject.Resources/Resource.es-ES.resx` (Spanish)

Key pattern: `ExceptionCode_<Module>_<EntityName>_DuplicateName`

## Step 17: Run the EF Core Migration

**IMPORTANT:** Do NOT auto-generate this file. Instruct the user to run:

```bash
cd src
dotnet ef migrations add <MigrationName> --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

## Checklist

- [ ] Entity DTO created with `IEntity`, `RowVersion`, DataAnnotations
- [ ] Summary DTO created with `ISummary`
- [ ] SearchRequest subclass created (only if extra filters needed)
- [ ] Database entity created with `Tb` prefix, `[Table]`, `[Key]`
- [ ] EntityTypeConfiguration created with indexes
- [ ] DbSet added to ApplicationDbContext
- [ ] Mapster config created (only if property names differ)
- [ ] Service interface created with `ITransientService` + generic contracts
- [ ] Service implementation with permission check, validation, CRUD
- [ ] Summary service interface and implementation created
- [ ] CRUD controller created with proper route and attributes
- [ ] Summary controller created
- [ ] ExceptionCode added
- [ ] Resource keys added to all three `.resx` files
- [ ] User instructed to run EF Core migration
