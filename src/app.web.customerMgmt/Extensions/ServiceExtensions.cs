using System;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using NLog.Web;
using NLog.Extensions.Logging;

using Microsoft.AspNetCore.Identity;
using Npgsql.EntityFrameworkCore.PostgreSQL;
using app.identity;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Builder;
using IdentityModel.AspNetCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

namespace app.web.customerMgmt.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureLogger(this IServiceCollection services)
        {            
            // Configure Logger
            var loggerFactory = new LoggerFactory();
            
            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties =true });
            NLog.LogManager.LoadConfiguration("NLog.config");            

            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
        }

        public static void ConfigureIdentity(this IServiceCollection services, IConfiguration Configuration)
        {            
            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseNpgsql(Configuration.GetConnectionString("IdentityDbConnection")));                     

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

        }

        public static void ConfigureAuth(this IServiceCollection services, IConfiguration Configuration)
        { 
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // Configuring Authorization to use Identity Server
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies")
            .AddOpenIdConnect("oidc", options =>
            {
                // Refer to https://hts.readthedocs.io/en/latest/configuration/mvc.html

                options.SignInScheme = "Cookies";

                options.Authority = Configuration["AUTH_SERVER"];
                options.RequireHttpsMetadata = false;

                // Use the same name as used in IdentityServer Config as ClientId
                options.ClientId = "customermgmt";
                options.ClientSecret = "49C1A7E1-0C79-4A89-A3D6-A37998FB86B0";
                options.ResponseType = "code id_token";
                options.GetClaimsFromUserInfoEndpoint = true;                
                options.SaveTokens = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };

                options.Scope.Clear();
                options.Scope.Add("openid");
                options.Scope.Add("profile");                
                options.Scope.Add("email");
                options.Scope.Add("address");
                options.Scope.Add("phone");
                // options.Scope.Add("api-customers");

            });


            services.ConfigureApplicationCookie(options =>
            {                
                options.Cookie.HttpOnly = true;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.LoginPath = "/Account/Signin";
                options.LogoutPath = "/Account/Signout";
                options.Cookie = new CookieBuilder
                {
                    IsEssential = true // required for auth to work without explicit user consent; adjust to suit your privacy policy
                };
            });
        }

    }
}                