using System.Net;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Deed.Application.Abstractions.Behaviours;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Exchanges.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

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

        var auth0 = services.BuildServiceProvider().GetRequiredService<IOptions<AuthSettings>>().Value;

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = AuthConstants.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = environment.IsDevelopment()
                ? CookieSecurePolicy.SameAsRequest
                : CookieSecurePolicy.Always;

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
            options.Authority = auth0.Domain;
            options.ClientId = auth0.ClientId;
            options.ClientSecret = auth0.ClientSecret;
            options.ResponseType = AuthConstants.ResponseType;
            options.SaveTokens = true;

            options.Scope.Clear();
            options.Scope.Add("openid");
            options.Scope.Add("profile");
            options.Scope.Add("email");

            options.TokenValidationParameters = new()
            {
                NameClaimType = "sub"
            };

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
        services.Configure<BankSettings>(configuration.GetRequiredSection(nameof(BankSettings)));
        
        services.Configure<WebUrlSettings>(configuration.GetRequiredSection(nameof(WebUrlSettings)));

        services.Configure<BackgroundJobsSettings>(configuration.GetRequiredSection(nameof(BackgroundJobsSettings)));

        services.Configure<MemoryCacheSettings>(configuration.GetRequiredSection(nameof(MemoryCacheSettings)));

        services.Configure<AuthSettings>(configuration.GetRequiredSection(nameof(AuthSettings)));

        services.Configure<SmtpSettings>(configuration.GetRequiredSection(nameof(SmtpSettings)));

        return services;
    }
}
