---
mode: agent
description: "Create a complete CRUD (backend + frontend) for a new entity in the AppProject .NET template"
---

# Create Full CRUD

Create a complete CRUD implementation for a new entity in the AppProject .NET template, covering both backend and frontend.

## Required Information

Determine the following before starting:
1. **Entity name** (e.g., `Product`, `Customer`, `Invoice`)
2. **Module name** (e.g., `General`, `Finance`, `Inventory`) — or if a new module is needed
3. **Entity fields** (name, type, required/optional, max length, foreign keys)
4. **Permission type** to use (existing or new)
5. **Whether summary needs aggregated fields** from related entities

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `new-module when needed, new-permission when needed, backend-crud, frontend-crud, localization, and unit-testing` skills from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Apply the skills in dependency order, create the complete vertical slice, and run the common completion checklist. Never create migration or snapshot files; report the manual migration command.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
