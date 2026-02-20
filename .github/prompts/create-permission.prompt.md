---
mode: agent
description: "Create a new permission type for controlling access to a module or feature"
---

# Create Permission

Create a new permission type in the AppProject .NET template.

## Required Information

You MUST ask the user for:
1. **Module name** (e.g., `Finance`, `HR`, `Inventory`)
2. **Action name** (e.g., `ManageInvoices`, `ManageEmployees`, `ViewReports`)
3. **Which services** should use this permission
4. **Which menu items** should be gated by this permission

## Instructions

Follow the `new-permission` skill to create the permission:

1. **Add enum value** to `src/AppProject.Models/Auth/PermissionType.cs`:
   - Follow `Module_Action` naming pattern
   - Use the next available sequential number
   - Example: `Finance_ManageInvoices = 2`

2. **Add permission validation** to service methods:
   ```csharp
   await permissionService.ValidateCurrentUserPermissionAsync(
       PermissionType.<Module>_<Action>,
       cancellationToken: cancellationToken);
   ```

3. **Update NavMenu.razor** to conditionally show menu items based on the permission

4. **Add resource translations** to all three `.resx` files for the permission name and description

## Naming Convention

Pattern: `<Module>_<Action>`
- `System_ManageSettings` (existing)
- `Finance_ManageInvoices` (example)
- `HR_ManageEmployees` (example)

## Final Output

List the modified files and the new permission type value.
