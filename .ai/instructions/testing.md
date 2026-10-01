# Testing Instructions

- Backend unit tests use NUnit, Moq, Shouldly, and Bogus.
- Follow Arrange, Act, Assert.
- Cover successful behavior, validation failures, not-found behavior, permission
  checks, persistence calls, and entity-specific business rules.
- Verify critical mock calls and use callbacks when the persisted value itself
  must be asserted.
- Build the affected projects and run the most focused relevant tests first.
- Run the complete solution suite when the change has cross-project impact.

From the repository root, the standard commands are:

```bash
dotnet build src/AppProject.slnx
dotnet test src/AppProject.slnx
```
