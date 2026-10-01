---
mode: agent
description: "Add a new database entity with EF Core configuration, DbSet, and prepare for migration"
---

# Add Database Entity

Create a new database entity with EF Core EntityTypeConfiguration, DbSet registration, and Mapster configuration.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Invoice`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Table columns** (name, type, required/optional, max length)
4. **Foreign keys** (parent entities and cardinality)
5. **Indexes** (unique, composite, regular)
6. **Whether Mapster config is needed** (custom property mapping)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `database-entities` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the entity and related configuration. Follow the canonical database instructions. Never create migration or snapshot files; report the manual migration command.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
