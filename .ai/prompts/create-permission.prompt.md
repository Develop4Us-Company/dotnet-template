---
mode: agent
description: "Create a new permission type for controlling access to a module or feature"
---

# Create Permission

Create a new permission type in the AppProject .NET template.

## Required Information

Determine the following:
1. **Module name** (e.g., `Finance`, `HR`, `Inventory`)
2. **Action name** (e.g., `ManageInvoices`, `ManageEmployees`, `ViewReports`)
3. **Which services** should use this permission
4. **Which menu items** should be gated by this permission

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `new-permission` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Inspect the current permission enum, add a stable explicit value without renumbering existing values, update enforcement and navigation, and add applicable resources.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
