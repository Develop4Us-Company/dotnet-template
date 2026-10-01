---
mode: agent
description: "Create a form/registration page for an existing entity"
---

# Create Form Page

Create a form (registration/edit) page for an entity that already has a backend and Refit client implementation.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Form fields** (labels, types, validations, max lengths)
4. **Whether the entity has FK fields** requiring dropdown selectors
5. **Whether the entity has nested/child items** (like City → Neighborhoods)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `frontend-crud` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the form flow using the applicable framework base page and controls, then add all required resources and validation.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
