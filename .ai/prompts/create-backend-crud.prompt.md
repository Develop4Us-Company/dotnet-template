---
mode: agent
description: "Create a backend-only CRUD for a new entity (DTOs, database entity, services, controllers)"
---

# Create Backend CRUD

Create the backend (API) side of a CRUD for a new entity in the AppProject .NET template.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Entity fields** (name, type, required/optional, max length, foreign keys)
4. **Permission type** to use
5. **Whether summary needs aggregated fields** from related entities

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `backend-crud` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the complete backend flow and follow the common completion checklist. Never create migration or snapshot files; report the manual migration command.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
