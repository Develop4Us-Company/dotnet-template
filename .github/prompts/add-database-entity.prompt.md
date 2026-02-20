---
mode: agent
description: "Add a new database entity with EF Core configuration, DbSet, and prepare for migration"
---

# Add Database Entity

Create a new database entity with EF Core EntityTypeConfiguration, DbSet registration, and Mapster configuration.

## Required Information

You MUST ask the user for:
1. **Entity name** (e.g., `Product`, `Invoice`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Table columns** (name, type, required/optional, max length)
4. **Foreign keys** (parent entities and cardinality)
5. **Indexes** (unique, composite, regular)
6. **Whether Mapster config is needed** (custom property mapping)

## Instructions

Follow the `database-entities` skill to create:

1. **Database entity** at `src/AppProject.Core.Infrastructure.Database/Entities/<Module>/Tb<Entity>.cs`
   - `[Table("PluralName")]`, `[Key]`, `[Required]`, `[MaxLength]`
   - `[ForeignKey]` with navigation properties
   - `ICollection<T>` for child navigations
   - Inherit from `BaseEntity`

2. **EntityTypeConfiguration** at `src/AppProject.Core.Infrastructure.Database/EntityTypeConfiguration/<Module>/Tb<Entity>Configuration.cs`
   - Unique indexes, composite indexes, search indexes

3. **DbSet** in `ApplicationDbContext`
   - Plural name convention

4. **Mapster config** (only if needed) at `src/AppProject.Core.Infrastructure.Database/Mapper/<Module>/`

## CRITICAL

**NEVER auto-generate EF Core migration files.** After creating the entity, remind the user to run:

```bash
cd src
dotnet ef migrations add <MigrationName> --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```
