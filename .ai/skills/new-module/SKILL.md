---
name: new-module
description: Step-by-step instructions for creating a brand new module in the AppProject .NET template. Covers creating all backend and frontend projects, registering assemblies in bootstrap files, configuring lazy loading, and adding menu items. Use when the user needs to create a completely new module (not just a new entity in an existing module).
metadata:
  author: appproject
  version: "1.0"
---

# Creating a New Module

Follow these steps when you need to create an entirely new module (not just a new entity in an existing module like General). Use the **General** module as the reference.

## Step 1: Create Backend Projects

Create the following projects (replace `<NewModule>` with the module name):

### 1.1 Models Project
```
src/AppProject.Core.Models.<NewModule>/AppProject.Core.Models.<NewModule>.csproj
```
Reference: `AppProject.Core.Models.General.csproj`

### 1.2 Services Project
```
src/AppProject.Core.Services.<NewModule>/AppProject.Core.Services.<NewModule>.csproj
```
Reference: `AppProject.Core.Services.General.csproj`

### 1.3 Controllers Project
```
src/AppProject.Core.Controllers.<NewModule>/AppProject.Core.Controllers.<NewModule>.csproj
```
Reference: `AppProject.Core.Controllers.General.csproj`

### 1.4 Database Folders
Create inside `src/AppProject.Core.Infrastructure.Database/`:
- `Entities/<NewModule>/`
- `EntityTypeConfiguration/<NewModule>/`
- `Mapper/<NewModule>/` (only if custom Mapster configs needed)

### 1.5 Shared Service Folder
```
src/AppProject.Core.Services/<NewModule>/
```
For summary services shared across modules.

### 1.6 Shared Models Folder
```
src/AppProject.Core.Models/<NewModule>/
```
For summary DTOs and search requests shared across modules.

## Step 2: Create Frontend Projects

### 2.1 Web Pages Project
```
src/AppProject.Web.<NewModule>/AppProject.Web.<NewModule>.csproj
```
Reference: `AppProject.Web.General.csproj`

### 2.2 API Client Project
```
src/AppProject.Web.ApiClient.<NewModule>/AppProject.Web.ApiClient.<NewModule>.csproj
```
Reference: `AppProject.Web.ApiClient.General.csproj`

### 2.3 Web Models Project
```
src/AppProject.Web.Models.<NewModule>/AppProject.Web.Models.<NewModule>.csproj
```
Reference: `AppProject.Web.Models.General.csproj`

### 2.4 Shared Folders
- `src/AppProject.Web.ApiClient/<NewModule>/` — shared Refit summary clients
- `src/AppProject.Web.Models/<NewModule>/` — shared observable summary models
- `src/AppProject.Web.Shared/<NewModule>/Components/` — shared dropdown/card components

## Step 3: Create Test Projects

### 3.1 Backend Tests
```
src/AppProject.Core.Tests.<NewModule>/AppProject.Core.Tests.<NewModule>.csproj
```
Reference: `AppProject.Core.Tests.General.csproj`

### 3.2 Frontend Tests (Optional)
```
src/AppProject.Web.Tests.<NewModule>/AppProject.Web.Tests.<NewModule>.csproj
```

## Step 4: Register Backend Assemblies

### 4.1 Bootstrap.cs

**File:** `src/AppProject.Core.API/Bootstraps/Bootstrap.cs`

Add the controller assembly to `GetControllerAssemblies()`:
```csharp
Assembly.Load("AppProject.Core.Controllers.<NewModule>"),
```

Add the service assembly to `GetServiceAssemblies()`:
```csharp
Assembly.Load("AppProject.Core.Services.<NewModule>"),
```

Note: The shared service assembly `AppProject.Core.Services` is already registered, so summary services placed there are auto-discovered.

## Step 5: Register Frontend Assemblies

### 5.1 WebBootstrap.cs

**File:** `src/AppProject.Web/Bootstraps/WebBootstrap.cs`

Add to `GetApiClientAssemblies()`:
```csharp
Assembly.Load("AppProject.Web.ApiClient.<NewModule>"),
```

Note: The shared `AppProject.Web.ApiClient` assembly is already registered.

### 5.2 App.razor — Lazy Loading

**File:** `src/AppProject.Web/App.razor`

In `OnNavigateAsync`, add assembly loading for the new module's route prefix:
```csharp
if (args.Path.StartsWith("<newmodule_lowercase>"))
{
    var assemblies = new List<Assembly> { typeof(AppProject.Web.<NewModule>.Pages.<SomePage>).Assembly };
    additionalAssemblies.AddRange(assemblies);
}
```

### 5.3 AppProject.Web.csproj

**File:** `src/AppProject.Web/AppProject.Web.csproj`

Add project references:
```xml
<ProjectReference Include="..\AppProject.Web.<NewModule>\AppProject.Web.<NewModule>.csproj" />
<ProjectReference Include="..\AppProject.Web.ApiClient.<NewModule>\AppProject.Web.ApiClient.<NewModule>.csproj" />
<ProjectReference Include="..\AppProject.Web.Models.<NewModule>\AppProject.Web.Models.<NewModule>.csproj" />
```

Add lazy load entry:
```xml
<BlazorWebAssemblyLazyLoad Include="AppProject.Web.<NewModule>.dll" />
```

## Step 6: Add Menu Items

**File:** `src/AppProject.Web/Layout/NavMenu.razor`

Add navigation items for the new module with the appropriate permission check:
```razor
@if (hasPermission_<NewModule>)
{
    <RadzenPanelMenuItem Text=@StringResource.GetStringByKey("NavMenu_<NewModule>_Title") Icon="category">
        <RadzenPanelMenuItem Text=@StringResource.GetStringByKey("NavMenu_<NewModule>_<EntityName>_Text")
            Path="<newmodule_lowercase>/<plural_entity_lowercase>" Icon="list" />
    </RadzenPanelMenuItem>
}
```

## Step 7: Add Resource Keys

Add translation keys to all three `.resx` files for:
- Menu items: `NavMenu_<NewModule>_Title`, `NavMenu_<NewModule>_<EntityName>_Text`
- Page titles, column titles, field labels, validators
- Exception messages

## Step 8: Add to Solution

Add all new projects to the solution file:
```bash
cd src
dotnet sln AppProject.slnx add AppProject.Core.Models.<NewModule>/AppProject.Core.Models.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Core.Services.<NewModule>/AppProject.Core.Services.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Core.Controllers.<NewModule>/AppProject.Core.Controllers.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Web.<NewModule>/AppProject.Web.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Web.ApiClient.<NewModule>/AppProject.Web.ApiClient.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Web.Models.<NewModule>/AppProject.Web.Models.<NewModule>.csproj
dotnet sln AppProject.slnx add AppProject.Core.Tests.<NewModule>/AppProject.Core.Tests.<NewModule>.csproj
```

## Important Notes

- Before adding `ProjectReference`, verify if the dependency is already accessible via shared assemblies (e.g., `AppProject.Core.Services` already references Jobs, so new modules don't need to reference Jobs directly).
- Controller routes should use the pattern `api/<newmodule_lowercase>/[controller]/[action]`.
- Blazor page routes should use the pattern `/<newmodule_lowercase>/<plural_entity_lowercase>`.
- All code, file names, and comments must be in English.

## Checklist

- [ ] Backend projects created (Models, Services, Controllers)
- [ ] Database folders created (Entities, EntityTypeConfiguration, Mapper)
- [ ] Shared folders created (Services, Models)
- [ ] Frontend projects created (Web, ApiClient, Models)
- [ ] Shared frontend folders created (ApiClient, Models, Shared components)
- [ ] Test projects created
- [ ] Backend assemblies registered in Bootstrap.cs
- [ ] Frontend assemblies registered in WebBootstrap.cs
- [ ] Lazy loading configured in App.razor
- [ ] Project references and lazy load entries in Web.csproj
- [ ] Menu items added to NavMenu.razor
- [ ] Resource keys added to all `.resx` files
- [ ] Projects added to solution file
