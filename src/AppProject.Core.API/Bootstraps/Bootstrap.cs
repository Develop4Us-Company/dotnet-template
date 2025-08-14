using System;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AppProject.Core.API.Auth;
using AppProject.Core.API.EmailRenderer;
using AppProject.Core.API.Middlewares;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using AppProject.Core.Infrastructure.Email;
using AppProject.Core.Services;
using AppProject.Exceptions;
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

                c.OAuthClientId(auth0Options.ClientId);
                c.OAuthAppName("API - Swagger");
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");

                c.OAuthScopes("openid", "profile", "email", "offline_access");

                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                {
                    { "audience", auth0Options.Audience }
                });
            });
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

        if (systemAdminUserOptions is null
            || string.IsNullOrEmpty(systemAdminUserOptions.Name)
            || string.IsNullOrEmpty(systemAdminUserOptions.Email))
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
                Name = systemAdminUserOptions.Name!,
                Email = systemAdminUserOptions.Email!,
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

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        if (string.IsNullOrEmpty(auth0Options.Authority) || string.IsNullOrEmpty(auth0Options.Audience))
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
            options.Authority = $"{auth0Options.Authority}";
            options.Audience = auth0Options.Audience;

            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = $"{auth0Options.Authority}",
                ValidateAudience = true,
                ValidAudience = auth0Options.Audience,
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
                        AuthorizationUrl = new Uri($"{auth0Options.Authority}/authorize"),
                        TokenUrl = new Uri($"{auth0Options.Authority}/oauth/token"),
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

            options.ApiKey = sendEmailOptions.ApiKey;
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
        public string Authority { get; set; } = string.Empty;

        public string ClientId { get; set; } = string.Empty;

        public string Audience { get; set; } = string.Empty;
    }

    private class SystemAdminUserOptions
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
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
