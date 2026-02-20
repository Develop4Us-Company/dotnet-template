---
name: database-entities
description: Detailed instructions for creating and managing database entities, EF Core EntityTypeConfiguration, ApplicationDbContext DbSets, Mapster configurations, and running migrations in the AppProject .NET template. Use when the user needs to work with database entities, EF Core configurations, indexes, migrations, or entity-to-DTO mapping.
metadata:
  author: appproject
  version: "1.0"
---

# Database Entities & EF Core Configuration

## Database Entity Pattern

**Location:** `src/AppProject.Core.Infrastructure.Database/Entities/<Module>/`

All database entities follow the `Tb[Name]` naming convention and inherit from `BaseEntity`.

### Simple Entity (no FK)

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

    [MaxLength(200)]
    public string? Code { get; set; }

    // Navigation collection for children:
    public ICollection<TbChild> Children { get; set; } = new List<TbChild>();
}
```

### Entity with Foreign Key

```csharp
[Table("<PluralName>")]
public class Tb<EntityName> : BaseEntity
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    public Guid ParentEntityId { get; set; }

    [ForeignKey(nameof(ParentEntityId))]
    public TbParentEntity ParentEntity { get; set; } = default!;

    // Child navigation:
    public ICollection<TbChild> Children { get; set; } = new List<TbChild>();
}
```

### Important Rules
- Use `[Table("PluralName")]` — always plural table names
- Use `[Key]` on primary key (`Guid Id`)
- Use `[Required]` and `[MaxLength]` on text columns
- Use `[ForeignKey(nameof(...))]` for navigation properties
- Initialize `ICollection<T>` with `new List<T>()`
- Inherit from `BaseEntity` (provides `RowVersion` and audit fields)
- Store only persistence data — business logic stays in services

## EntityTypeConfiguration

**Location:** `src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/<Module>/`

All configurations implement `IEntityTypeConfiguration<T>` and are **auto-discovered** by `ApplicationDbContext.OnModelCreating()` — no manual registration needed.

### Basic Configuration (unique index)

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
    }
}
```

### Configuration with Multiple Indexes

```csharp
public void Configure(EntityTypeBuilder<Tb<EntityName>> builder)
{
    builder.HasIndex(x => x.Name);
    builder.HasIndex(x => x.ParentEntityId);
    // Composite index:
    builder.HasIndex(x => new { x.Name, x.ParentEntityId }).IsUnique();
}
```

## Adding DbSet to ApplicationDbContext

**File:** `src/AppProject.Core.Infrastructure.Database/ApplicationDbContext.cs`

Add a `DbSet<T>` with a **plural name**:

```csharp
public DbSet<Tb<EntityName>> <PluralName> { get; set; } = default!;
```

Keep the convention: `Countries`, `States`, `Cities`, `Neighborhoods`, etc.

## Mapster Configuration

**Location:** `src/AppProject.Core.Infrastructure.Database/Mapper/<Module>/`

Only create when property names between entity and DTO differ. Common case: mapping navigation property names to summary DTOs.

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

**Auto-discovered** by `Bootstrap.ConfigureMapper()` — no manual registration.

### When to Create Mapster Config
- **DO create** when summary DTO has fields from navigation properties (e.g., `CountryName` from `Country.Name`)
- **DO NOT create** when all property names match between entity and DTO (Mapster handles this automatically)
- **Avoid** complex logic in Mapster configs — use services for business rules

## Running Migrations

**CRITICAL: NEVER auto-generate EF Core migration or snapshot files.**

Always instruct the user to run the command manually:

```bash
cd src
dotnet ef migrations add <MigrationName> --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

Replace `<MigrationName>` with a descriptive name (e.g., `AddProductTable`, `AddInvoiceIndexes`).

**Notes:**
- The migration is applied automatically when the API starts — no need to run `dotnet ef database update`
- Make sure `dotnet-ef` tool is installed: `dotnet tool install --global dotnet-ef`
- The `--startup-project` must point to `AppProject.Core.API` for connection string resolution

## Relationship Patterns

### One-to-Many (Parent → Children)
```csharp
// Parent entity
public ICollection<TbChild> Children { get; set; } = new List<TbChild>();

// Child entity
public Guid ParentId { get; set; }
[ForeignKey(nameof(ParentId))]
public TbParent Parent { get; set; } = default!;
```

### Cascade Delete
EF Core handles cascade delete automatically for required relationships. For optional relationships, configure in `EntityTypeConfiguration`:
```csharp
builder.HasOne(x => x.Parent)
    .WithMany(x => x.Children)
    .HasForeignKey(x => x.ParentId)
    .OnDelete(DeleteBehavior.Restrict);
```

## Checklist

- [ ] Entity created with `Tb` prefix, `BaseEntity`, `[Table]`, `[Key]`
- [ ] `[MaxLength]` applied to all text columns
- [ ] Foreign keys with `[ForeignKey]` and navigation properties
- [ ] Navigation collections initialized with `new List<T>()`
- [ ] EntityTypeConfiguration with appropriate indexes
- [ ] DbSet added to ApplicationDbContext with plural name
- [ ] Mapster config created only when property names differ
- [ ] User instructed to run migration command manually
