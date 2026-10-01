---
mode: agent
description: "Add localization resource keys to all .resx files for a new entity or feature"
---

# Add Resource Keys

Add localization resource keys to all three `.resx` files for a new entity, page, or feature.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Customer`)
2. **Module name** (e.g., `General`, `Finance`)
3. **What needs translations:** page titles, column names, field labels, validators, menu items, exception messages, or all of them
4. **Field names** and their expected labels in English, Portuguese, and Spanish (or let me provide sensible translations)

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `localization` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Add the required keys to all three resource files while preserving their grouping, comments, formatting, and placeholder rules.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
