using Deed.Application.Abstractions.Behaviours;
using Deed.Application.Abstractions.Settings;
using Deed.Application.Auth;
using Deed.Application.Auth.Picture;
using Deed.Application.BankConnections.Service;
using Deed.Application.Exchanges;
using Deed.Application.Exchanges.Service;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Deed.Application;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication(IConfiguration configuration)
        {
            services.AddSettings(configuration);

            services.AddAuth();

            services.AddMediatrDependencies();

            services.AddScoped<IExchangeRateProvider, ExchangeRateProvider>();

            services.AddValidatorsFromAssembly(AssemblyReference.Assembly, includeInternalTypes: true);

            services.AddHttpClient<IExchangeHttpService, ExchangeHttpService>()
                .AddStandardResilienceHandler();

            services.AddHttpClient<IProfilePictureService, ProfilePictureService>(client => 
                client.Timeout = TimeSpan.FromSeconds(10));
            
            services.AddHttpClient<IPrivatAutoclientService, PrivatAutoclientService>()
                .AddStandardResilienceHandler();

            services.AddHttpClient<IPrivatCardService, PrivatCardService>()
                .AddStandardResilienceHandler();

            services.AddScoped<IBankTransactionSyncService, BankTransactionSyncService>();

            return services;
        }

        private void AddAuth()
        {
            services.AddScoped<IUser, User>();

            AuthSettings authSettings = services.BuildServiceProvider().GetRequiredService<IOptions<AuthSettings>>().Value;

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
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                })
                .AddOpenIdConnect(AuthConstants.AuthenticationScheme, options =>
                {
                    options.Authority = authSettings.Domain;
                    options.ClientId = authSettings.ClientId;
                    options.ClientSecret = authSettings.ClientSecret;
                    options.ResponseType = AuthConstants.ResponseType;
                    options.SaveTokens = true;
                    options.MapInboundClaims = false;

                    options.Scope.Clear();
                    options.Scope.Add("openid");
                    options.Scope.Add("profile");
                    options.Scope.Add("email");
                    options.Scope.Add("offline_access");
                
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "https://deed-app/roles"
                    };


                    options.Events.OnRedirectToIdentityProvider = ctx =>
                    {
                        ctx.ProtocolMessage.SetParameter("audience", authSettings.Audience);
                    
                        var isExplicitLogin = ctx.Properties.Items.ContainsKey(AuthConstants.ExplicitLoginKey);
                        if (ctx.Request.Path.StartsWithSegments("/api") && !isExplicitLogin)
                        {
                            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                            ctx.HandleResponse();
                        }

                        return Task.CompletedTask;
                    };
                });

            services.AddAuthorizationBuilder()
                .AddPolicy(AuthConstants.AdminOnlyPolicy, p => p.RequireRole(AuthRolesConstants.Admin));
        }

        private IServiceCollection AddMediatrDependencies()
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(AssemblyReference.Assembly);

                config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));

                config.AddOpenBehavior(typeof(RequestLoggingPipelineBehavior<,>));
            });

            return services;
        }

        private IServiceCollection AddSettings(IConfiguration configuration)
        {
            services.Configure<ForwardedHeadersOptions>(o =>
            {
                o.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                                     | ForwardedHeaders.XForwardedProto
                                     | ForwardedHeaders.XForwardedHost;
                o.KnownIPNetworks.Clear();
                o.KnownProxies.Clear();
            });

            services.Configure<BankSettings>(configuration.GetRequiredSection(nameof(BankSettings)));
        
            services.Configure<WebUrlSettings>(configuration.GetRequiredSection(nameof(WebUrlSettings)));

            services.Configure<BackgroundJobsSettings>(configuration.GetRequiredSection(nameof(BackgroundJobsSettings)));

            services.Configure<MemoryCacheSettings>(configuration.GetRequiredSection(nameof(MemoryCacheSettings)));

            services.Configure<AuthSettings>(configuration.GetRequiredSection(nameof(AuthSettings)));

            services.Configure<SmtpSettings>(configuration.GetRequiredSection(nameof(SmtpSettings)));

            services.Configure<PrivatAutoclientSettings>(configuration.GetSection(nameof(PrivatAutoclientSettings)));

            services.Configure<PrivatCardSettings>(configuration.GetSection(nameof(PrivatCardSettings)));

            return services;
        }
    }
}
