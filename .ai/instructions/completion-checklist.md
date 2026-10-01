# Completion Checklist

Before reporting a task as complete:

1. Confirm that the requested behavior is fully implemented.
2. Confirm that only relevant files changed.
3. Confirm that no EF Core migration or snapshot file was created or edited.
4. Build the affected projects.
5. Run the relevant tests and, when appropriate, the complete solution suite.
6. Confirm resource parity when localization changed.
7. Confirm bootstrap, lazy-loading, navigation, and project registration when a
   module or project changed.
8. Review `git diff` and `git diff --check`.
9. Report changed files, commands run, failures, and environment limitations
   accurately.
