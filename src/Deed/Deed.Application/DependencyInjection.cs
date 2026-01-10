using Deed.Application.Abstractions.Behaviours;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Exchanges.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Deed.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSettings();

        services.AddAuth();

        services.AddMediatrDependencies();

        services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

        services.AddHttpClient<IExchangeHttpService, ExchangeHttpService>();

        return services;
    }

    private static void AddAuth(this IServiceCollection services)
    {
        services.AddSingleton<IUser, User>();

        var authSettings = services.BuildServiceProvider()
            .GetRequiredService<IOptions<AuthSettings>>().Value;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = AuthConstants.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.None;
            options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

            options.ExpireTimeSpan = TimeSpan.FromHours(1);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = ctx =>
            {
                if (ctx.Request.Path.StartsWithSegments("/api"))
                {
                    ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    return Task.CompletedTask;
                }
                ctx.Response.Redirect(ctx.RedirectUri);
                return Task.CompletedTask;
            };
        })
        .AddOpenIdConnect(AuthConstants.AuthenticationScheme, options =>
        {
            options.Authority = authSettings.Domain;
            options.ClientId = authSettings.ClientID;
            options.ClientSecret = authSettings.ClientSecret;

            options.ResponseType = AuthConstants.ResponseType;
            options.SaveTokens = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.CallbackPath = new PathString("/api/auth/callback");
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

    private static IServiceCollection AddSettings(this IServiceCollection services)
    {
        var configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.Configure<WebUrlSettings>(configuration.GetRequiredSection(nameof(WebUrlSettings)));

        services.Configure<BackgroundJobsSettings>(configuration.GetRequiredSection(nameof(BackgroundJobsSettings)));

        services.Configure<MemoryCacheSettings>(configuration.GetRequiredSection(nameof(MemoryCacheSettings)));

        services.Configure<AuthSettings>(configuration.GetRequiredSection(nameof(AuthSettings)));

        return services;
    }
}
