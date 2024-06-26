﻿using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using ids.Entities;

namespace ids.TestData
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email(),
            new IdentityResource
            {
                Name = "role",
                UserClaims= new List<string> {"role"}
            },
            new IdentityResource{
                Name="customerNumber",
                UserClaims=new List<string> {"customerNumber"}
            } };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
            new ApiScope("messagesapi.read"),
            new ApiScope("messagesapi.write"),
            };

        public static IEnumerable<ApiResource> ApiResources => new[]
        {
            new ApiResource("messagesapi")
            {
                Scopes = new List<string>{"messagesapi.read", "messagesapi.write"},
                ApiSecrets = new List<Secret>{new Secret("ScopeSecret".Sha256())},
                UserClaims= new List<string> {"role", "customerNumber"}
            }
        };

        public static IEnumerable<Client> Clients =>
            new Client[]
            {
            // m2m client credentials flow client
                new Client
                {
                    ClientId = "m2m.client", //Machine to machine
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "messagesapi.read", "messagesapi.write" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "interactive",
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:5444/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:5444/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:5444/signout-callback-oidc" },
                    //RequireClientSecret = false,
                    //RequirePkce =true,
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "messagesapi.read", CustomJwtClaimTypes.CustomerNumber },
                    RequireConsent = true            
                },
                new Client
                {
                    ClientId = "spa",
                    ClientSecrets = { new Secret("b03651e6-ab90-4f8e-ad65-4e5d6f6f2dd1".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "http://localhost:5173/signin-oidc" },
                    FrontChannelLogoutUri = "http://localhost:5173/signout-oidc",
                    PostLogoutRedirectUris = { "http://localhost:5173/signout-callback-oidc" },
                    RequireClientSecret = false,
                    RequirePkce =true,
                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "messagesapi.read", IdentityServerConstants.StandardScopes.Email },
                    RequireConsent = true,
                    AllowRememberConsent = false,
                }
            };
    }

}
