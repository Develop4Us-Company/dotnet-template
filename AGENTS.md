# Repository Agent Instructions

The canonical agent knowledge base for this repository is [`.ai/`](.ai/README.md).
This file is only the Codex-compatible entry point and must not become a separate
copy of the project guidance.

Before changing code:

1. Read [`.ai/instructions/repository.md`](.ai/instructions/repository.md).
2. Read the instruction documents relevant to the task.
3. Use the applicable skill exposed through [`.agents/skills/`](.agents/skills/).
4. Inspect the closest existing implementation before creating a new pattern.
5. Follow [the common completion checklist](.ai/instructions/completion-checklist.md).

Critical bootstrap rules: never create or edit EF Core migration or snapshot
files; keep code and identifiers in English; keep all three localization files
synchronized; and build and test affected code before reporting completion.
