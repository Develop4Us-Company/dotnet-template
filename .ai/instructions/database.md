# Database Instructions

- Use `IDatabaseRepository` for all service data access.
- Keep entities under
  `src/AppProject.Core.Infrastructure.Database/Entities/<Module>/` and their
  `IEntityTypeConfiguration<T>` implementations under the corresponding
  `EntityTypeConfiguration/<Module>/` folder.
- Register plural `DbSet` properties in `ApplicationDbContext`.
- Define indexes and relationship behavior explicitly when the domain requires
  them.
- Preserve optimistic concurrency through the existing `RowVersion` pattern.
- Use save methods for simple operations and a single explicit `SaveAsync` for
  an atomic multi-entity operation.

## Migrations

Never create, edit, or regenerate EF Core migration or snapshot files. After a
schema change, instruct the user to run the following command from `src/`:

```bash
dotnet ef migrations add <MigrationName> --project AppProject.Core.Infrastructure.Database --startup-project AppProject.Core.API --output-dir Migrations
```

The application applies migrations automatically at API startup. Production
deployment must still ensure that migrations are applied using the deployment's
chosen strategy.
