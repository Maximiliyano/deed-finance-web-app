using Deed.Application.Abstractions.Behaviours;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Exchanges.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace Deed.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        services.AddSettings(configuration);

        services.AddAuth(environment);

        services.AddMediatrDependencies();

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

        services.AddHttpClient<IExchangeHttpService, ExchangeHttpService>()
            .AddStandardResilienceHandler();

        return services;
    }

    private static void AddAuth(this IServiceCollection services, IWebHostEnvironment environment)
    {
        services.AddScoped<IUser, User>();

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = AuthConstants.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            // In development (HTTP), SameSite=None requires Secure which browsers reject over HTTP.
            // localhost is same-site regardless of port, so Lax works for dev.
            // In production the frontend and API are on different domains, so None+Secure is required.
            options.Cookie.SameSite = environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None;
            options.Cookie.SecurePolicy = environment.IsDevelopment() ? CookieSecurePolicy.SameAsRequest : CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = async ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await ctx.Response.CompleteAsync();
                    return;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
            };
        })
        .AddOpenIdConnect(AuthConstants.AuthenticationScheme, options =>
        {
            options.ResponseType = AuthConstants.ResponseType;
            options.SaveTokens = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.Events.OnRedirectToIdentityProvider = ctx =>
            {
                var isExplicitLogin = ctx.Properties.Items.ContainsKey(AuthConstants.ExplicitLoginKey);
                if (ctx.Request.Path.StartsWithSegments("/api") && !isExplicitLogin)
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    ctx.HandleResponse();
                }
                return Task.CompletedTask;
            };
        });

        services.AddOptions<OpenIdConnectOptions>(AuthConstants.AuthenticationScheme)
            .Configure<IOptions<AuthSettings>>((oidcOptions, authSettingsOptions) =>
            {
                var authSettings = authSettingsOptions.Value;
                oidcOptions.Authority = authSettings.Domain;
                oidcOptions.ClientId = authSettings.ClientID;
                oidcOptions.ClientSecret = authSettings.ClientSecret;
            });

        services.AddAuthorization();
    }

    private static IServiceCollection AddMediatrDependencies(this IServiceCollection services)
    {
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(AssemblyReference.Assembly);

            config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));

            config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
        });

        return services;
    }

    private static IServiceCollection AddSettings(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            // KnownNetworks and KnownProxies are intentionally cleared so the app trusts
            // X-Forwarded-Proto from the Fly.io / Azure reverse proxy regardless of its IP.
            // Ensure the deployment environment is the only network entry point.
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.Configure<WebUrlSettings>(configuration.GetRequiredSection(nameof(WebUrlSettings)));

        services.Configure<BackgroundJobsSettings>(configuration.GetRequiredSection(nameof(BackgroundJobsSettings)));

        services.Configure<MemoryCacheSettings>(configuration.GetRequiredSection(nameof(MemoryCacheSettings)));

        services.Configure<AuthSettings>(configuration.GetRequiredSection(nameof(AuthSettings)));

        services.Configure<SmtpSettings>(configuration.GetRequiredSection(nameof(SmtpSettings)));

        return services;
    }
}
