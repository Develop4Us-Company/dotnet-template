---
mode: agent
description: "Create unit tests for a CRUD service using NUnit, Moq, Shouldly, and Bogus"
---

# Create Unit Tests

Create unit tests for an existing CRUD service and its summary service in the AppProject .NET template.

## Required Information

Determine the following:
1. **Entity name** (e.g., `Product`, `Country`)
2. **Module name** (e.g., `General`, `Finance`)
3. **Which services to test** (CRUD service, summary service, or both)
4. **Any specific business rules** beyond the standard duplicate check

## Workflow

1. Read `.ai/instructions/repository.md` and the instruction documents relevant to the task.
2. Use the canonical `unit-testing` skill from `.ai/skills/`.
3. Determine the required information from the request and repository; ask only for information that is missing or ambiguous.
4. Create the requested test coverage for the existing implementation and run the focused tests.
5. Follow `.ai/instructions/completion-checklist.md` and report the files changed and checks run.
