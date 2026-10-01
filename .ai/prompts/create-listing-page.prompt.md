---
mode: agent
description: "Create a listing/search page for an existing entity"
---

# Create Listing Page

Create a search/listing page for an entity that already has a backend and Refit client implementation.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Columns to display** in the data grid
4. **Whether advanced filters are needed** (e.g., filter by parent entity)
5. **Whether a custom SearchRequest** exists or use base `SearchRequest`

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `frontend-crud` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the listing flow using the framework search page and controls, then add the applicable resources, filters, actions, and navigation.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
