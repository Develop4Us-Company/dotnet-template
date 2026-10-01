# Canonical Agent Knowledge Base

This directory is the single source of truth for coding-agent guidance in this
repository. Tool-specific entry points must stay small and refer here instead of
duplicating these rules.

## Instructions

Read [`instructions/repository.md`](instructions/repository.md) before making any
change, then load the instruction documents relevant to the task:

- [`instructions/architecture.md`](instructions/architecture.md)
- [`instructions/coding-conventions.md`](instructions/coding-conventions.md)
- [`instructions/configuration.md`](instructions/configuration.md)
- [`instructions/database.md`](instructions/database.md)
- [`instructions/localization.md`](instructions/localization.md)
- [`instructions/testing.md`](instructions/testing.md)
- [`instructions/completion-checklist.md`](instructions/completion-checklist.md)

## Reusable workflows

- [`skills/`](skills/) contains the canonical agent skills.
- [`prompts/`](prompts/) contains explicit task entry points for clients that
  support prompt files.

The `General` module is the reference implementation. Instructions and examples
must be checked against the current source before they are applied.
