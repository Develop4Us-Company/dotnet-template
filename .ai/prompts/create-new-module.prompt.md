---
mode: agent
description: "Create a new module with all required projects, folders, and registrations"
---

# Create New Module

Create a brand new module in the AppProject .NET template with all required projects, folders, and bootstrap registrations.

## Required Information

Determine the following:
1. **Module name** (e.g., `Finance`, `HR`, `Inventory`)
2. **Initial entities** to create in the module
3. **Permission name** for the module

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `new-module` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create and register the requested projects and folders, then use the CRUD skills for the initial entities.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
