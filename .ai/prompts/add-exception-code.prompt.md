---
mode: agent
description: "Add a new ExceptionCode for business rule validation"
---

# Add Exception Code

Add a new `ExceptionCode` for business rule validation in the AppProject .NET template.

## Required Information

Determine the following:
1. **Module name** (e.g., `General`, `Finance`)
2. **Entity name** (e.g., `Country`, `Product`)
3. **Validation name** (e.g., `DuplicateName`, `InvalidStatus`, `ExceedsLimit`)
4. **Error messages** in English, Portuguese, and Spanish

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `backend-crud and localization` skills from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Inspect the current `ExceptionCode` enum, add the business exception, add all localized resources, and update its use site.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
