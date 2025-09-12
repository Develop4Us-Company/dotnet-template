using System;
using System.Globalization;
using System.Reflection;
using AppProject.Web.Constants;
using AppProject.Web.Options;
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

        var host = builder.Build();

        await SetLanguageAsync(host);

        await host.RunAsync();
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

        builder.Services.Configure<WebAuth0Options>(builder.Configuration.GetSection("Auth0"));
    }

    private static void ConfigureRefit(WebAssemblyHostBuilder builder)
    {
        var refitInterfaces = GetApiClientAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(x => x.IsInterface);

        var apiOptions = new ApiOptions();
        builder.Configuration.GetSection("Api").Bind(apiOptions);

        if (string.IsNullOrEmpty(apiOptions.BaseUrl))
        {
            throw new InvalidOperationException("Api:BaseUrl configuration is missing.");
        }

        foreach (var refitInterface in refitInterfaces)
        {
            builder.Services
                .AddRefitClient(refitInterface)
                .ConfigureHttpClient(c =>
                {
                    c.BaseAddress = new Uri(apiOptions.BaseUrl);
                })
                .AddHttpMessageHandler(sp => sp.GetService<AuthorizationMessageHandler>()
                    .ConfigureHandler(authorizedUrls: new[] { apiOptions.BaseUrl }));
        }
    }

    private static async Task SetLanguageAsync(WebAssemblyHost host)
    {
        var storagedLanguage = await host.Services.GetRequiredService<ILocalStorageService>()
            .GetItemAsync<string>(AppProjectConstants.LanguageLocalStorageKey);

        var culture = string.IsNullOrEmpty(storagedLanguage)
            ? new CultureInfo(AppProjectConstants.DefaultLanguage)
            : new CultureInfo(storagedLanguage);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
    }

    private static IEnumerable<Assembly> GetApiClientAssemblies() =>
        [
            Assembly.Load("AppProject.Web.ApiClient"),
            Assembly.Load("AppProject.Web.ApiClient.General"),
        ];

    private class ApiOptions
    {
        public string BaseUrl { get; set; }
    }
}
