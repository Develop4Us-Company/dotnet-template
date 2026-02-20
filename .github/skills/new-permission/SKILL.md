---
name: new-permission
description: Step-by-step instructions for creating a new permission type in the AppProject .NET template. Covers adding the permission enum value, updating services, configuring menu visibility, and adding resource translations. Use when the user needs to create a new permission for controlling access to a module or feature.
metadata:
  author: appproject
  version: "1.0"
---

# Creating a New Permission

Follow these steps to add a new permission type that controls access to a module or feature.

## Overview

The permission system uses:
- `PermissionType` enum in `src/AppProject.Models/Auth/PermissionType.cs`
- `IPermissionService.ValidateCurrentUserPermissionAsync()` for backend validation
- `IPermissionClient` on the frontend for menu visibility
- `NavMenu.razor` for conditional rendering of menu items

## Step 1: Add the PermissionType Enum Value

**File:** `src/AppProject.Models/Auth/PermissionType.cs`

Add a new enum value following the `Module_Action` naming pattern:

```csharp
namespace AppProject.Models.Auth;

public enum PermissionType
{
    // Format: Module_Action
    System_ManageSettings = 1,

    // Add new permission:
    <Module>_<Action> = <NextNumber>,
}
```

**Naming conventions:**
- Use `PascalCase` with underscore separating module and action
- Examples: `Finance_ManageInvoices`, `HR_ManageEmployees`, `Inventory_ViewReports`
- The numeric value must be unique and sequential

## Step 2: Use in Backend Services

In your service methods, validate the permission before executing any operation:

```csharp
await permissionService.ValidateCurrentUserPermissionAsync(
    PermissionType.<Module>_<Action>,
    cancellationToken: cancellationToken);
```

This call throws `AppException(ExceptionCode.SecurityValidation)` if the user lacks the permission.

## Step 3: Update NavMenu for Frontend Visibility

**File:** `src/AppProject.Web/Layout/NavMenu.razor`

Add a permission check variable and use it for conditional rendering:

```razor
@code {
    private bool hasPermission_<Module> = false;

    protected override async Task OnInitializedAsync()
    {
        // Add permission check
        hasPermission_<Module> = await this.CheckPermissionAsync(PermissionType.<Module>_<Action>);
    }
}
```

Then conditionally render the menu items:

```razor
@if (hasPermission_<Module>)
{
    <RadzenPanelMenuItem Text=@StringResource.GetStringByKey("NavMenu_<Module>_Title") Icon="category">
        <RadzenPanelMenuItem Text=@StringResource.GetStringByKey("NavMenu_<Module>_Entity_Text")
            Path="<module_lowercase>/<entities_lowercase>" Icon="list" />
    </RadzenPanelMenuItem>
}
```

## Step 4: Add Resource Translations

Add resource keys to all three `.resx` files for any UI text related to the permission:

**Files:**
- `src/AppProject.Resources/Resource.resx` (English)
- `src/AppProject.Resources/Resource.pt-BR.resx` (Portuguese)
- `src/AppProject.Resources/Resource.es-ES.resx` (Spanish)

Keys to add:
- `Permission_<Module>_<Action>_Name` — Human-readable permission name
- `Permission_<Module>_<Action>_Description` — Permission description

## Step 5: Configure Permission Assignment

Ensure the permission can be assigned to users/roles through the permission management system. The admin user (`SystemAdminUser`) configured in `appsettings.json` automatically has access to all permissions.

## Reference: Existing Permissions

```csharp
public enum PermissionType
{
    System_ManageSettings = 1,
}
```

The `System_ManageSettings` permission is used by all General module services (Country, State, City).

## Checklist

- [ ] `PermissionType` enum value added with unique number
- [ ] Permission validation added to all relevant service methods
- [ ] `NavMenu.razor` updated with permission check
- [ ] Resource translations added to all three `.resx` files
- [ ] Permission can be assigned to users/roles
