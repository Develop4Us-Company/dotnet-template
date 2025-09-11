using System;
using AppProject.Web.Pages;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;
using Refit;

namespace AppProject.Web.Bootstraps;

public static class WebBootstrap
{
    public static async Task ConfigureAndRunAsync(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);
        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        ConfigureLocalization(builder);

        ConfigureLocalStorage(builder);

        ConfigureRadzen(builder);

        ConfigureAuthentication(builder);

        ConfigureRefit(builder);

        await builder.Build().RunAsync();
    }

    private static void ConfigureLocalization(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddLocalization();
    }

    private static void ConfigureLocalStorage(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddBlazoredLocalStorage();
    }

    private static void ConfigureRadzen(WebAssemblyHostBuilder builder)
    {
        builder.Services.AddRadzenComponents();
    }

    private static void ConfigureAuthentication(WebAssemblyHostBuilder builder)
    {
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
    }

    private static void ConfigureRefit(WebAssemblyHostBuilder builder)
    {
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
                    .ConfigureHandler(authorizedUrls: new[] { "https://localhost:7121" });

                return handler;
            });
    }
}
