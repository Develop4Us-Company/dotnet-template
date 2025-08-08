using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using AppProject.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

using AppProject.Web.Pages;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configuração do Auth0
builder.Services.AddOidcAuthentication(options =>
{
    builder.Configuration.Bind("Auth0", options.ProviderOptions);
    options.ProviderOptions.ResponseType = "code";
    options.ProviderOptions.DefaultScopes.Add("openid");
    options.ProviderOptions.DefaultScopes.Add("profile");
    options.ProviderOptions.DefaultScopes.Add("email");
    options.ProviderOptions.DefaultScopes.Add("offline_access");

    options.ProviderOptions.AdditionalProviderParameters.Add("audience", builder.Configuration["Auth0:Audience"]);

});

// Configuração do Refit
builder.Services
    .AddRefitClient<ISampleClient>()
    .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri("https://localhost:7121");
                })
                 .AddHttpMessageHandler(sp =>
                 {
                     var handler = sp.GetService<AuthorizationMessageHandler>()
                     .ConfigureHandler(
                         authorizedUrls: new[] { "https://localhost:7121" }
                      );
                     return handler;
                 });


await builder.Build().RunAsync();
