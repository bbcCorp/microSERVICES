﻿// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

using app.identity;

namespace app.services.sts
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var dbConnectionStr = Configuration.GetConnectionString("IdentityDbConnection");
            if (String.IsNullOrEmpty(dbConnectionStr))
            {
                throw new Exception("Please set the DB connection string for Identity Database");
            }
            System.Console.WriteLine($"IdentityDB: {dbConnectionStr}");

            services.AddDbContext<AppIdentityDbContext>(options =>
                options.UseNpgsql(dbConnectionStr));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<AppIdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddMvc();

            services.Configure<IISOptions>(iis =>
            {
                iis.AuthenticationDisplayName = "Windows";
                iis.AutomaticAuthentication = false;
            });

            var builder = services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;
            })
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryApiResources(Config.GetApis())
                .AddInMemoryClients(Config.GetClients(Configuration))
                .AddAspNetIdentity<ApplicationUser>();

            // Create the initial set of IDs if it does not exist
            SeedData.EnsureSeedData(services.BuildServiceProvider());                

            if (Environment.IsDevelopment())
            {
                builder.AddDeveloperSigningCredential();                
            }
            else
            {
                throw new Exception("need to configure key material");
            }

            services.AddAuthentication();
                // .AddGoogle(options =>
                // {
                //     options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                //     options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                // });
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
