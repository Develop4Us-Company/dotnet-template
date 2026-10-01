# External integrations

The sections below describe the registrations required so every integration works. After completing each step, copy the values to the `appsettings` files.

## Auth0
1. Create an application of type **Single Page Application**.
2. Configure the application logo in *Settings* if you want.
3. Fill in the URLs (adjust the ports if you change `launchSettings.json`):
   - **Allowed Callback URLs**: `https://localhost:7035/authentication/login-callback`, `https://localhost:7121/swagger/oauth2-redirect.html`
   - **Allowed Logout URLs**: `https://localhost:7035`, `https://localhost:7121/swagger/`
   - **Allowed Web Origins**: `https://localhost:7035`, `https://localhost:7121`
4. Create an **API** in Auth0 and use as the *Identifier* the same value configured in `Auth0:Audience` (`https://appproject.api` by default). Then go to **Access Settings** and check **Allow Offline Access**.
5. To include `email`, `name`, and `roles` in the JWT, create a `post_login` Action with the script below:
   ```javascript
   if (api.accessToken) {
       if (event.user && event.user.email) {
         api.accessToken.setCustomClaim("email", event.user.email);
       }

       if (event.user && event.user.name) {
         api.accessToken.setCustomClaim("name", event.user.name);
       }

       if (event.authorization && event.authorization.roles) {
         api.accessToken.setCustomClaim("roles", event.authorization.roles);
       }
     }
   ```
6. Copy `Authority` and `ClientId` from the Single Page Application you created and copy the API `Audience` to the `appsettings`, keeping the `https://` prefix for both `Authority` and `Audience`. For example:
   ```json
   "Authority": "https://yourauth0domain.us.auth0.com",
   "ClientId": "yourclientid",
   "Audience": "https://youraudience.com"
   ```
> Note: When opening Swagger, clear the browser cache so it does not reuse parameters from other projects.

## SendGrid
1. Create an account on the [SendGrid website](https://sendgrid.com/).
2. Configure an identity (domain authentication or single sender). Authorize the identity through the email you receive.
3. In the **Email API > Integration Guide** menu, generate an API Key.
4. Send the first test email and confirm the delivery in the dashboard.
5. Copy the key and the configured sender (`SendEmail:ApiKey`, `SendEmail:FromEmailAddress`, `SendEmail:FromName`).
6. When creating new emails, model the body in a Razor template (`SampleEmailTemplate.cshtml` and `SampleEmailModel` in `AppProject.Core.Infrastructure.Email` are examples) and only fill the model inside the service, avoiding inline strings.

## GitHub AI Models
1. Follow the official documentation: <https://docs.github.com/en/github-models/use-github-models/prototyping-with-ai-models>.
2. Generate a token with permission to use the models hosted by GitHub at [https://github.com/settings/tokens](https://github.com/settings/tokens).
3. Fill `AI:Endpoint` (default `https://models.github.ai/inference`) and `AI:Token` with the generated token.

## Administrator user
When the API runs for the first time, the bootstrap creates or updates the administrator user defined in `SystemAdminUser`. Use this user to ensure at least one account has permission to access every registry.
