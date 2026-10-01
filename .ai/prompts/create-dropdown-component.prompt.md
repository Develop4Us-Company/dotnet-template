---
mode: agent
description: "Add a new dropdown data grid component for selecting a related entity"
---

# Create Dropdown Component

Create a reusable dropdown data grid component (`DropDownDataGridControl`) for selecting a related entity in forms and search filters.

## Required Information

Determine the following:
1. **Entity name** whose summaries will be listed (e.g., `Country`, `State`, `Product`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Display columns** in the dropdown grid (usually just `Name`)
4. **Whether the value is required** (`Guid`) or optional (`Guid?`)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `shared-components` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the reusable dropdown using the closest existing component as the implementation reference, then validate all affected projects.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
