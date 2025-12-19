using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AppProject.Core.API.Auth;
using AppProject.Core.API.EmailRenderer;
using AppProject.Core.API.Middlewares;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.AI;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using AppProject.Core.Infrastructure.Database.Mapper;
using AppProject.Core.Infrastructure.Email;
using AppProject.Core.Infrastructure.Jobs;
using AppProject.Core.Services;
using AppProject.Exceptions;
using Hangfire;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.OpenApi.Models;
using SendGrid.Extensions.DependencyInjection;
using Serilog;

namespace AppProject.Core.API.Bootstraps;

public static class Bootstrap
{
    private const string DefaultCorsPolicyName = "DefaultCorsPolicy";

    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
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

        ConfigureSwagger(builder);

        ConfigureCache(builder);

        ConfigureLog(builder);

        ConfigureCors(builder);

        ConfigureRateLimiting(builder);

        ConfigureEmail(builder);

        ConfigureAI(builder);

        ConfigureJobs(builder);

        return builder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseRequestLocalization();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");

                var auth0Options = new Auth0Options();
                app.Configuration.GetSection("Auth0").Bind(auth0Options);

                var audience = auth0Options.Audience;
                var clientId = auth0Options.ClientId;

                if (string.IsNullOrEmpty(audience)
                    || string.IsNullOrEmpty(clientId))
                {
                    throw new ArgumentException("Auth0 is not configured properly.");
                }

                c.OAuthClientId(clientId);
                c.OAuthAppName("API - Swagger");
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");

                c.OAuthScopes("openid", "profile", "email", "offline_access");

                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                {
                    { "audience", audience }
                });
            });

            app.UseHangfireDashboard("/hangfire");
        }
        else
        {
            app.UseHsts();
        }

        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "no-referrer";
            context.Response.Headers["Permissions-Policy"] = "geolocation=(), microphone=(), camera=()";
            await next();
        });

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseCors(DefaultCorsPolicyName);

        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseMiddleware<SerilogUserEnricherMiddleware>();

        JobsBootstrap.RegisterRecurringJobs();

        app.MapControllers();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        await applicationDbContext.Database.MigrateAsync();
    }

    public static async Task CreateOrUpdateSystemAdminUserAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var systemAdminUserOptions = new SystemAdminUserOptions();
        app.Configuration.GetSection("SystemAdminUser").Bind(systemAdminUserOptions);

        if (string.IsNullOrWhiteSpace(systemAdminUserOptions.Name)
            || string.IsNullOrWhiteSpace(systemAdminUserOptions.Email))
        {
            throw new ArgumentException("SystemAdminUser configuration is not set properly.");
        }

        var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin);

        if (user == null)
        {
            var adminUserId = Guid.NewGuid();

            user = new TbUser
            {
                Id = adminUserId,
                Name = systemAdminUserOptions.Name,
                Email = systemAdminUserOptions.Email,
                IsSystemAdmin = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUserId,
                CreatedByUserName = systemAdminUserOptions.Name!
            };

            applicationDbContext.Users.Add(user);
            await applicationDbContext.SaveChangesAsync();
        }
        else if (user.Name != systemAdminUserOptions.Name || user.Email != systemAdminUserOptions.Email)
        {
            user.Name = systemAdminUserOptions.Name!;
            user.Email = systemAdminUserOptions.Email!;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = user.Id;
            user.UpdatedByUserName = user.Name;

            applicationDbContext.Users.Update(user);
            await applicationDbContext.SaveChangesAsync();
        }

        var hybridCache = scope.ServiceProvider.GetRequiredService<HybridCache>();
        await hybridCache.RemoveAsync(CacheKeys.SystemAdminUserKey);
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
            var supportedCultures = new[] { "en-US", "pt-BR", "es-ES" };
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
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
            .WithScopedLifetime());

        builder.Services.Scan(x =>
            x.FromAssemblies(GetServiceAssemblies())
            .AddClasses(y =>
                y.AssignableTo<ISingletonService>())
            .AsImplementedInterfaces()
            .WithSingletonLifetime());
    }

    private static void ConfigureUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddHttpContextAccessor();
    }

    private static void ConfigureMapper(WebApplicationBuilder builder)
    {
        builder.Services.AddMapster();

        builder.Services.Scan(scan => scan
            .FromAssemblyOf<IRegisterMapsterConfig>()
            .AddClasses(classes => classes.AssignableTo<IRegisterMapsterConfig>())
            .As<IRegisterMapsterConfig>()
            .WithSingletonLifetime());

        var provider = builder.Services.BuildServiceProvider();
        var configs = provider.GetServices<IRegisterMapsterConfig>();

        var config = TypeAdapterConfig.GlobalSettings;

        foreach (var mapConfig in configs)
        {
            mapConfig.Register(config);
        }

        builder.Services.AddSingleton(config);
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

        var connectionStringsOptions = new ConnectionStringsOptions();
        builder.Configuration.GetSection("ConnectionStrings").Bind(connectionStringsOptions);

        var databaseConnection = connectionStringsOptions.DatabaseConnection;
        if (string.IsNullOrWhiteSpace(databaseConnection))
        {
            throw new ArgumentException("Database connection string is not configured.");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(x =>
            x.UseSqlServer(
                    databaseConnection,
                    y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
    }

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        var authority = auth0Options.Authority;
        var audience = auth0Options.Audience;

        if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(audience))
        {
            throw new ArgumentException("Auth0 configuration is not set properly.");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;

            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        var authority = auth0Options.Authority;

        if (string.IsNullOrWhiteSpace(authority))
        {
            throw new ArgumentException("Auth0 configuration is not set properly.");
        }

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1"
            });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{authority}/authorize?prompt=login"),
                        TokenUrl = new Uri($"{authority}/oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" },
                            { "offline_access", "Offline Access" }
                        }
                    }
                },
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "OAuth2 with Auth0"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email", "offline_access" }
                }
            });
        });
    }

    private static void ConfigureCache(WebApplicationBuilder builder)
    {
        builder.Services.AddHybridCache();

        // You can configure IDistributedCache here if needed and connect with Redis or other cache providers
    }

    private static void ConfigureLog(WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Configuration)
            .Enrich.FromLogContext()
            .CreateLogger();

        builder.Logging.AddSerilog(Log.Logger);

        // You can configure Application Insights or other logging providers here
    }

    private static void ConfigureCors(WebApplicationBuilder builder)
    {
        var corsOptions = new CorsOptions();
        builder.Configuration.GetSection("Cors").Bind(corsOptions);

        if (corsOptions.AllowedOrigins?.Any() == false)
        {
            throw new ArgumentException("CORS allowed origins are not configured.");
        }

        builder.Services.AddCors(options =>
        {
            options.AddPolicy(DefaultCorsPolicyName, policy =>
            {
                policy.WithOrigins(corsOptions?.AllowedOrigins ?? Array.Empty<string>())
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });
    }

    private static void ConfigureRateLimiting(WebApplicationBuilder builder)
    {
        var rateOptions = new RateLimitingOptions();
        builder.Configuration.GetSection("RateLimiting").Bind(rateOptions);

        if (rateOptions.PermitLimit <= 0 || rateOptions.WindowSeconds <= 0 || rateOptions.QueueLimit < 0)
        {
            throw new ArgumentException("Rate limiting options are not configured properly.");
        }

        builder.Services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = rateOptions.PermitLimit,
                        Window = TimeSpan.FromSeconds(rateOptions.WindowSeconds),
                        QueueLimit = rateOptions.QueueLimit,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));
        });
    }

    private static void ConfigureEmail(WebApplicationBuilder builder)
    {
        builder.Services.Configure<SendEmailOptions>(builder.Configuration.GetSection("SendEmail"));
        builder.Services.AddScoped<IEmailSender, EmailSender>();
        builder.Services.AddScoped<IEmailTemplateRenderer, EmailTemplateRenderer>();
        builder.Services.AddSendGrid(options =>
        {
            var sendEmailOptions = new SendEmailOptions();
            builder.Configuration.GetSection("SendEmail").Bind(sendEmailOptions);

            var apiKey = sendEmailOptions.ApiKey;

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new ArgumentException("SendEmail API key must be configured.");
            }

            options.ApiKey = apiKey;
        });
    }

    private static void ConfigureAI(WebApplicationBuilder builder)
    {
        builder.Services.Configure<AIOptions>(builder.Configuration.GetSection("AI"));
        builder.Services.AddScoped<IChatClient, ChatClient>();
    }

    private static void ConfigureJobs(WebApplicationBuilder builder)
    {
        builder.Services.AddHangfire(config =>
        {
            var hangfireConnection = builder.Configuration.GetConnectionString("HangfireConnection");

            if (string.IsNullOrWhiteSpace(hangfireConnection))
            {
                throw new ArgumentException("Hangfire connection string is not configured.");
            }

            config.UseSqlServerStorage(hangfireConnection);
        });

        builder.Services.AddHangfireServer();

        builder.Services.AddScoped<IJobDispatcher, JobDispatcher>();
    }

    private static IEnumerable<Assembly> GetControllerAssemblies() =>
        [
            Assembly.Load("AppProject.Core.Controllers.General"),
        ];

    private static IEnumerable<Assembly> GetServiceAssemblies() =>
        [
            Assembly.Load("AppProject.Core.Services"),
            Assembly.Load("AppProject.Core.Services.General")
        ];

    private class ConnectionStringsOptions
    {
        public string? DatabaseConnection { get; set; }
    }

    private class Auth0Options
    {
        public string? Authority { get; set; }

        public string? ClientId { get; set; }

        public string? Audience { get; set; }
    }

    private class SystemAdminUserOptions
    {
        public string? Name { get; set; }

        public string? Email { get; set; }
    }

    private class CorsOptions
    {
        public string[] AllowedOrigins { get; set; } = Array.Empty<string>();
    }

    private class RateLimitingOptions
    {
        public int PermitLimit { get; set; }

        public int WindowSeconds { get; set; }

        public int QueueLimit { get; set; }
    }
}
