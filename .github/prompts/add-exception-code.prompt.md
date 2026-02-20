---
mode: agent
description: "Add a new ExceptionCode for business rule validation"
---

# Add Exception Code

Add a new `ExceptionCode` for business rule validation in the AppProject .NET template.

## Required Information

You MUST ask the user for:
1. **Module name** (e.g., `General`, `Finance`)
2. **Entity name** (e.g., `Country`, `Product`)
3. **Validation name** (e.g., `DuplicateName`, `InvalidStatus`, `ExceedsLimit`)
4. **Error messages** in English, Portuguese, and Spanish

## Instructions

1. **Add enum value** to `src/AppProject.Exceptions/ExceptionCode.cs`:
   ```csharp
   // Pattern: Module_Entity_ValidationName
   <Module>_<Entity>_<ValidationName>,
   ```

2. **Add resource keys** to all three `.resx` files:
   - Key: `ExceptionCode_<Module>_<Entity>_<ValidationName>`
   - `Resource.resx` — English message
   - `Resource.pt-BR.resx` — Portuguese message
   - `Resource.es-ES.resx` — Spanish message

3. **Use in service** validation:
   ```csharp
   throw new AppException(ExceptionCode.<Module>_<Entity>_<ValidationName>);
   ```

## Existing Exception Codes

```csharp
Generic,
SecurityValidation,
RequestValidation,
Concurrency,
EntityNotFound,
General_Country_DuplicateName,
General_State_DuplicateName,
General_City_DuplicateName,
General_City_Neighborhood_DuplicateName,
```
