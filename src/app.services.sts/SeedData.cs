// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using app.identity;

namespace app.services.sts
{
    public class SeedData
    {
        public static void EnsureSeedData(IServiceProvider serviceProvider)
        {
            using (var scope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var context = scope.ServiceProvider.GetService<AppIdentityDbContext>();
                // context.Database.Migrate();

                var userMgr = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                
                var admin = userMgr.FindByNameAsync("admin").Result;
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        UserName = "admin",
                        TwoFactorEnabled=false,
                        Email="bedabrata.chatterjee@gmail.com",EmailConfirmed=true,
                        PhoneNumber="1-800-ADMIN", PhoneNumberConfirmed=true
                    };
                    var result = userMgr.CreateAsync(admin, "Admin#123").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine($"User:admin created. ID:{admin.Id}");

                    Console.WriteLine($"Creating claims for User:admin");
                    result = userMgr.AddClaimsAsync(admin, new Claim[]{
                        new Claim("geo-location", "90.00000 0.00000"),
                        new Claim(JwtClaimTypes.Name, "Agent Smith"),
                        new Claim(JwtClaimTypes.GivenName, "admin"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "bedabrata.chatterjee@gmail.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "https://github.com/bbcCorp/microSERVICES"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One North Pole', 'locality': 'North Pole', 'postal_code': H0H 0H0, 'country': 'Earth' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("Claims created for User:admin");
                }
                else
                {
                    Console.WriteLine("User:admin already exists");
                }


                var bbc = userMgr.FindByNameAsync("bbc").Result;
                if (bbc == null)
                {
                    bbc = new ApplicationUser
                    {
                        UserName = "bbc",
                        TwoFactorEnabled=false,
                        Email="bedabrata.chatterjee@gmail.com",EmailConfirmed=true,
                        PhoneNumber="1-800-BBC", PhoneNumberConfirmed=true                        
                    };
                    var result = userMgr.CreateAsync(bbc, "P@ssword123").Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine($"User:bbc created. ID:{bbc.Id}");

                    Console.WriteLine("Adding claims for User:bbc");
                    result = userMgr.AddClaimsAsync(bbc, new Claim[]{
                        new Claim("geo-location", "13.05387 77.59879"),
                        new Claim(JwtClaimTypes.Name, "Bedabrata Chatterjee"),
                        new Claim(JwtClaimTypes.GivenName, "Bedabrata"),
                        new Claim(JwtClaimTypes.FamilyName, "Chatterjee"),
                        new Claim(JwtClaimTypes.Email, "bedabrata.chatterjee@gmail.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "https://github.com/bbcCorp"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Bangalore Way', 'locality': 'Bangalore', 'postal_code': 560024, 'country': 'India' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "Bangalore")
                    }).Result;
                    if (!result.Succeeded)
                    {
                        throw new Exception(result.Errors.First().Description);
                    }
                    Console.WriteLine("Claims created for User:bbc");
                }
                else
                {
                    Console.WriteLine("User:bbc already exists");
                }
            }
        }
    }
}
