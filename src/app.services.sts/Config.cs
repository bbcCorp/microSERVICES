// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace app.services.sts
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new IdentityResource[]
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new ApiResource[]
            {
                new ApiResource("api-customers", "microSERVICES Customer API")
            };
        }

        public static IEnumerable<Client> GetClients(IConfiguration configuration)
        {
            return new[]
            {
                // client credentials flow client
                new Client
                {
                    ClientId = configuration["ApiClient:ClientId"],
                    ClientName = configuration["ApiClient:ClientId"],

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "api-customers" }
                },

                // MVC client using hybrid flow
                new Client
                {
                    ClientId = configuration["WebClient:ClientId"],
                    ClientName = configuration["WebClient:ClientName"],

                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    RedirectUris = { configuration["WebClient:RedirectUris"] },
                    FrontChannelLogoutUri = configuration["WebClient:FrontChannelLogoutUri"],
                    PostLogoutRedirectUris = { configuration["WebClient:PostLogoutRedirectUris"] },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "api-customers" }
                },

                // // SPA client using implicit flow
                // new Client
                // {
                //     ClientId = "spa",
                //     ClientName = "SPA Client",
                //     ClientUri = "http://identityserver.io",

                //     AllowedGrantTypes = GrantTypes.Implicit,
                //     AllowAccessTokensViaBrowser = true,

                //     RedirectUris =
                //     {
                //         "http://localhost:5002/index.html",
                //         "http://localhost:5002/callback.html",
                //         "http://localhost:5002/silent.html",
                //         "http://localhost:5002/popup.html",
                //     },

                //     PostLogoutRedirectUris = { "http://localhost:5002/index.html" },
                //     AllowedCorsOrigins = { "http://localhost:5002" },

                //     AllowedScopes = { "openid", "profile", "api1" }
                // }
            };
        }
    }
}