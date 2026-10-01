# Quick guide to using the template

## Prerequisites
- .NET SDK (the `TargetFramework` is already defined in `src/Directory.Build.props`).
- Visual Studio or Visual Studio Code with the C# extension to work with the solution.
- SQL Server. You can spin up a local container with the command:
  ```bash
  docker run --name appproject-sqlserver -e 'ACCEPT_EULA=Y' -e 'SA_PASSWORD=yourStrong(!)Password' -p 1433:1433 -d mcr.microsoft.com/mssql/server:2022-latest
  ```
- Active Auth0, SendGrid, and OpenAI Platform accounts to fill in the integrations described below.
- Optional: install the global `dotnet-ef` tool to run migration commands (`dotnet tool install --global dotnet-ef`).
- Important: do not let Copilot, Codex, or any other generator automatically create EF Core snapshot or migration files. Run the migration script/command manually (for example, `dotnet ef migrations add ...`) to make sure the code is generated correctly.

## Step-by-step to set up the environment
1. Clone the repository and restore the dependencies with `dotnet restore AppProject.slnx`.
2. Verify the .NET installation with `dotnet --info` and confirm that .NET is installed.
3. Make sure the `src/AppProject.Core.API/appsettings.Development.json` file is configured to point to your local resources (for example, the connection string `Server=localhost,1433;Database=AppProject;...`) before running the API. The other values (Auth0, SendGrid, OpenAI API, etc.) also contain placeholders for you to fill in.
4. Configure the local (or containerized) SQL Server and validate the connection with `sqlcmd` or your tool of choice.
5. Fill in the placeholders in `src/AppProject.Core.API/appsettings.json` and `src/AppProject.Web/wwwroot/appsettings.json` before generating production builds. These files contain `<<SET_...>>` markers that flag what must be configured.
6. Configure the external integrations (Auth0, SendGrid, and OpenAI API) following the detailed instructions later on and copy the generated values into the configuration files.
7. Run the API with `dotnet run --project src/AppProject.Core.API` (default port `https://localhost:7121`).
8. Run the frontend with `dotnet run --project src/AppProject.Web` (default port `https://localhost:7035`).
9. Go to `https://localhost:7035` in your browser to use the application and to `https://localhost:7121/swagger` to test the endpoints.

## Configuration file checklist
- `src/AppProject.Core.API/appsettings.json` — base file used in production. Fill in the placeholders:
  - `<<SET_SQLSERVER_DATABASE_CONNECTION_STRING>>` and `<<SET_HANGFIRE_SQLSERVER_CONNECTION_STRING>>`: connection strings (they can be the same).
  - `<<SET_AUTH0_AUTHORITY>>`, `<<SET_AUTH0_CLIENT_ID>>`, `<<SET_AUTH0_AUDIENCE>>`: Auth0 application data.
  - `<<SET_SYSTEM_ADMIN_NAME>>`, `<<SET_SYSTEM_ADMIN_EMAIL>>`: administrator user that is created automatically.
  - `<<SET_ALLOWED_CORS_ORIGINS>>`: URLs allowed to consume the API.
  - `<<SET_ALLOWED_HOSTS>>`: hosts accepted when the application runs in production.
  - `<<SET_SENDGRID_API_KEY>>`, `<<SET_SENDGRID_FROM_EMAIL>>`, `<<SET_SENDGRID_FROM_NAME>>`: email sending credentials.
  - `<<SET_AI_ENDPOINT>>`, `<<SET_AI_TOKEN>>`: AI settings.
- `src/AppProject.Core.API/appsettings.Development.json` — already points to local connections (`Server=localhost,1433;...`) and keeps placeholders for sensitive credentials (Auth0, SendGrid, OpenAI API). Adjust it to your environment and avoid committing sensitive data.
- `src/AppProject.Web/wwwroot/appsettings.json` — frontend placeholders (`Auth0` and `Api:BaseUrl`). The published file must point to the production URLs.
- `src/AppProject.Web/wwwroot/appsettings.Development.json` — has `Api:BaseUrl` pointing to `https://localhost:7121` and keeps Auth0 placeholders.
- `src/AppProject.Web/Constants/AppProjectConstants.cs` — update the `ProjectName` constant with the name you want to use for your application/project.
- `src/AppProject.Web/Constants/ThemeConstants.cs` — keeps the theme storage keys aligned with the project name.
- `src/AppProject.Core.API/Bootstraps/Bootstrap.cs` — when creating new modules, register the assemblies in the `GetControllerAssemblies()` and `GetServiceAssemblies()` methods.
- `src/AppProject.Web/Bootstraps/WebBootstrap.cs` — include the Refit client assemblies in `GetApiClientAssemblies()` and validate `Api:BaseUrl`.
- `src/AppProject.Web/App.razor` — register additional assemblies in the `OnNavigateAsync` method to enable lazy loading for new modules.
- `src/AppProject.Web/AppProject.Web.csproj` — add new `ProjectReference` entries and `BlazorWebAssemblyLazyLoad` items when creating additional modules.
- `src/AppProject.Web/Layout/NavMenu.razor` — include menu items and permissions for the new modules.
- `src/AppProject.Resources/Resource*.resx` — keep translations in sync when adding new text. Preserve the comments that name each logical group, keep splitting entries per form (validations and menu remain grouped), and when you need to reserve future placeholders, use `{{}}` instead of `{}` so the parser keeps the literal text.
