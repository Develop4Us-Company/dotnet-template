# Preparing for production

- Fill every placeholder in `appsettings.json` and `wwwroot/appsettings.json` with real values (production connections, Auth0, SendGrid, OpenAI API, public URLs, etc.).
- Set `ASPNETCORE_ENVIRONMENT=Production` for the API and `DOTNET_ENVIRONMENT=Production` for the published frontend.
- Update `Cors:AllowedOrigins` and `AllowedHosts` with the official domains.
- Register the new URLs in Auth0 (callback, logout, and web origins) and generate a `ClientSecret` if needed.
- Ensure the production database is created and the migrations are applied (`dotnet ef database update` or automatic migration at startup).
- Generate a dedicated SendGrid key and validate the domain/sender used by the product.
- Create a dedicated OpenAI API key for the production environment, store it in the deployment platform's secret store, and configure `AI:Endpoint` as `https://api.openai.com/v1`.
- Adjust `SystemAdminUser` to an email that is actually monitored by the operations team.
- Review the logging configuration (`Serilog`) and consider sending logs to a persistent sink in production.
- Confirm whether Hangfire uses a separate database or a suitable connection string for the environment.
- Remove sample data and validate user permissions before go-live.
- Use environment variables or Azure App Configuration/Secrets Manager to store sensitive credentials, avoiding repository exposure.
- Run `dotnet publish -c Release src/AppProject.Core.API/AppProject.Core.API.csproj` and `dotnet publish -c Release src/AppProject.Web/AppProject.Web.csproj` to generate the artifacts that will be deployed to production environments.
- Configure pipelines (GitHub Actions, Azure DevOps, etc.) to run `dotnet test` and publish the projects automatically, ensuring migrations and configurations are applied before deployment.
