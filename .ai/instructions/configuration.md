# Configuration and Secret-Handling Instructions

- Never commit real credentials, access tokens, API keys, connection strings, or
  other secrets.
- Preserve `<<SET_...>>` markers in production configuration until deployment
  supplies the corresponding value.
- Use development configuration, .NET user secrets, environment variables, or
  the deployment platform's secret store for sensitive local and hosted values.
- Before changing an application URL or authentication setting, inspect both the
  API and frontend configuration because Auth0, CORS, Swagger, and Refit URLs
  must remain aligned.
- Keep `src/AppProject.Core.API/appsettings.json` production-safe and use
  `src/AppProject.Core.API/appsettings.Development.json` for local API settings.
- Keep `src/AppProject.Web/wwwroot/appsettings.json` production-safe and use
  `src/AppProject.Web/wwwroot/appsettings.Development.json` for local frontend
  settings.
- When creating an email, use a Razor template and populate its model in the
  service rather than building an inline body string.
- Do not remove or bypass the administrator bootstrap without replacing the
  guarantee that at least one account can manage permissions.

For setup values and integration-specific procedures, consult
[`../../docs/getting-started.md`](../../docs/getting-started.md) and
[`../../docs/integrations.md`](../../docs/integrations.md).
