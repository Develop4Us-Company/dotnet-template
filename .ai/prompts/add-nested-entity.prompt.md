---
mode: agent
description: "Add a nested/child entity to an existing parent form (like Neighborhoods in City)"
---

# Add Nested Entity

Add a nested/child entity to an existing parent entity's form, following the City → Neighborhoods pattern.

## Required Information

Determine the following:
1. **Parent entity name** (e.g., `City`, `Invoice`)
2. **Child entity name** (e.g., `Neighborhood`, `InvoiceItem`)
3. **Module name** (e.g., `General`, `Finance`)
4. **Child entity fields** (name, type, validations)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `backend-crud and frontend-crud` skills from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Follow the existing City and Neighborhood implementation for the complete backend, synchronization, client-model, and dialog workflow.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
