# Repository Instructions

These instructions apply to every coding task in this repository.

## Working approach

- Read the relevant canonical instructions and skill before editing code.
- Inspect the closest existing implementation, using the `General` module as the
  primary reference for CRUDs and modules.
- Make the smallest complete change and do not refactor unrelated code.
- Do not add a package or project dependency unless it is required.
- Before adding a `ProjectReference`, verify that the dependency is not already
  available through a shared assembly.
- Do not edit generated files.

## Language

- Code, file names, class names, identifiers, and code comments must be in English.
- Repository agent instructions and technical documentation are maintained in
  English.
- User-facing text must be localized through the resource files.
- Never commit credentials or replace production configuration placeholders with
  real secrets; follow `configuration.md` for configuration changes.

## Critical rules

- Never create or edit EF Core migration or snapshot files. Finish the source
  changes and tell the user which migration command to run manually.
- Use `IDatabaseRepository` for service data access; do not access the `DbContext`
  directly from services.
- Validate permissions in protected service operations with
  `IPermissionService.ValidateCurrentUserPermissionAsync`.
- Throw `AppException` with the appropriate `ExceptionCode` for business-rule
  violations.
- Apply duplicate checks when the entity has a business uniqueness rule.
- Keep the `en-US`, `pt-BR`, and `es-ES` resources synchronized.
- Complete the common checklist before reporting success.
