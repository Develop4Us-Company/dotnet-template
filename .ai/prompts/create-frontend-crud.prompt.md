---
mode: agent
description: "Create a frontend-only CRUD for an entity (web models, Refit clients, search page, form page)"
---

# Create Frontend CRUD

Create the frontend (Blazor WebAssembly) side of a CRUD for an entity that already has a backend implementation.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Entity fields** (to mirror in the observable model)
4. **API routes** (to match in the Refit client) — or verify from existing controllers
5. **Whether the entity has related/parent entities** (for dropdown components)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `frontend-crud` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the complete frontend flow, including models, clients, pages, resources, navigation, and registration that apply to the request.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
