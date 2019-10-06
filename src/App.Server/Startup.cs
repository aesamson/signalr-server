using System;
using System.Text;
using App.Server.Hubs;
using App.Server.Options;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace App.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(options =>
                    {
                        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(options => options.TokenValidationParameters =
                            new TokenValidationParameters
                            {
                                LifetimeValidator = (before, expires, token, param) => expires > DateTime.UtcNow,
                                ValidateAudience = false,
                                ValidateIssuer = false,
                                ValidateActor = false,
                                ValidateLifetime = true,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Auth:SecurityKey")))
                            });
            
            services.AddMvcCore();
            services.AddSignalR();
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAny", builder => builder
                    .AllowCredentials()
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .SetIsOriginAllowed(origin => true));
            });
            services.Configure<ChatOptions>(Configuration.GetSection("Chat"));
            services.Configure<AuthOptions>(Configuration.GetSection("Auth"));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();

            app.UseCors("AllowAny");

            app.UseAuthentication();
            app.UseSignalR(hubs =>
            {
                hubs.MapHub<ChatHub>("/signalr/chat");
            });
            app.UseMvc();
        }
    }
}