// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Test;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.Security.Claims;

namespace IdentityServer
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            //services.AddMvc().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_2_1);
            services.AddMvc();

            services
                .AddIdentityServer()
                .AddDeveloperSigningCredential()
                .AddInMemoryApiScopes(Config.GetApiScopes())
                .AddInMemoryApiResources(Config.GetApiResources())
                .AddInMemoryClients(Config.GetClients())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())    
                .AddTestUsers(new List<TestUser>
                {
                    new TestUser
                    {
                        SubjectId = "05AE9983-D580-4152-9FE0-A2C8B2E54B36",
                        Username = "user1",
                        Password = "password",

                        Claims = new[]
                        {
                            new Claim("name", "User1"),
                            new Claim("website", "http://www.eventstore.org"),
                        }
                    },
                    new TestUser
                    {
                        SubjectId = "8EB1D643-DD0D-41F6-AEC0-079D33F09A51",
                        Username = "user2",
                        Password = "password",

                        Claims = new[]
                        {
                            new Claim("name", "User2"),
                            new Claim("website", "http://www.eventstore.org"),
                        }
                    }
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseIdentityServer();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => endpoints.MapDefaultControllerRoute());
        }
    }
}