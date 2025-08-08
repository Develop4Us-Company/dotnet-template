using System;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using AppProject.Core.API.Auth;
using AppProject.Core.API.Middlewares;
using AppProject.Core.API.SettingOptions;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Services;
using AppProject.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace AppProject.Core.API.Bootstraps;

public static class Bootstrap
{
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();
        builder.Services.AddOpenApi();
        var mvcBuilder = builder.Services.AddControllers();

        ConfigureControllers(mvcBuilder);

        ConfigureLocalization(builder, mvcBuilder);

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            ConfigureValidations(options);
        });

        ConfigureServices(builder);

        ConfigureUsers(builder);

        ConfigureMapper(builder);

        ConfigureDatabase(builder);

        ConfigureAuthentication(builder);

        ConfigureLog(builder);

        ConfigureCors(builder);

        return builder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseRequestLocalization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseCors("AllowBlazor");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await applicationDbContext.Database.MigrateAsync();
    }

    private static void ConfigureControllers(IMvcBuilder mvcBuilder)
    {
        foreach (var assembly in GetControllerAssemblies())
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
    }

    private static void ConfigureLocalization(WebApplicationBuilder builder, IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddDataAnnotationsLocalization();

        builder.Services.AddLocalization();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "en-US", "pt-BR" };
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                    new QueryStringRequestCultureProvider(),
                    new CookieRequestCultureProvider(),
                    new AcceptLanguageHeaderRequestCultureProvider(),
            };
        });
    }

    private static void ConfigureValidations(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var modelErrors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er => er.ErrorMessage));

            var errors = modelErrors.Any() ? string.Join(" ", modelErrors) : null;
            throw new AppException(ExceptionCode.RequestValidation, errors);
        };
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Scan(x =>
            x.FromAssemblies(GetServiceAssemblies())
            .AddClasses(y =>
                y.AssignableTo<ITransientService>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        builder.Services.Scan(x =>
            x.FromAssemblies(GetServiceAssemblies())
            .AddClasses(y =>
                y.AssignableTo<IScopedService>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());

        builder.Services.Scan(x =>
            x.FromAssemblies(GetServiceAssemblies())
            .AddClasses(y =>
                y.AssignableTo<ISingletonService>())
            .AsImplementedInterfaces()
            .WithTransientLifetime());
    }

    private static void ConfigureUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddHttpContextAccessor();

        var systemAdminUserOptions = new SystemAdminUserOptions();
        builder.Configuration.GetSection("SystemAdminUser").Bind(systemAdminUserOptions);

        if (string.IsNullOrEmpty(systemAdminUserOptions.Name) || string.IsNullOrEmpty(systemAdminUserOptions.Email))
        {
            throw new ArgumentException("SystemAdmin user configuration is not set properly.");
        }

        builder.Services.AddSingleton(systemAdminUserOptions);
    }

    private static void ConfigureMapper(WebApplicationBuilder builder)
    {
        builder.Services.AddMapster();
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

        var connectionStringsOptions = new ConnectionStringsOptions();
        builder.Configuration.GetSection("ConnectionStrings").Bind(connectionStringsOptions);

        if (string.IsNullOrEmpty(connectionStringsOptions.DatabaseConnection))
        {
            throw new ArgumentException("Database connection string is not configured.");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(x =>
            x.UseSqlServer(
                connectionStringsOptions.DatabaseConnection,
                y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
    }

    private static void ConfigureAuthentication(this WebApplicationBuilder builder)
    {
        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = $"https://{auth0Options.Domain}/";
            options.Audience = auth0Options.Audience;

            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"https://{auth0Options.Domain}/",
                ValidateAudience = true,
                ValidAudience = auth0Options.Audience,
                ValidateLifetime = true,
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
    }

    private static void ConfigureLog(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger);
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowBlazor", policy =>
            {
                policy.WithOrigins("https://localhost:7035") // URL do Blazor WebAssembly
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials(); // necessário se estiver usando cookies ou tokens
            });
        });
    }

    private static IEnumerable<Assembly> GetControllerAssemblies() =>
        [
            Assembly.Load("AppProject.Core.Controllers.General"),
        ];

    private static IEnumerable<Assembly> GetServiceAssemblies() =>
        [
            Assembly.Load("AppProject.Core.Services"),
            Assembly.Load("AppProject.Core.Services.General"),
        ];

    private class ConnectionStringsOptions
    {
        public string DatabaseConnection { get; set; } = string.Empty;
    }

    private class Auth0Options
    {
        public string Domain { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;
    }
}
