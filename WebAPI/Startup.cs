using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using WebAPI.Data;

namespace WebAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticConfig = configuration;
        }

        public IConfiguration Configuration { get; }
        public static IConfiguration StaticConfig { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var mongoCon = Configuration.GetSection("ZaiZahDatabaseSettings")["ConnectionString"];
            services.AddSingleton<IMongoClient>(new MongoClient(mongoCon));
            services.AddSingleton<IMongoDbData, MongoDbData>();
            services.AddTransient<IProductRepo, ProductRepo>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddControllers();
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddCors();

            X509Certificate2 x509Certificate2 = new X509Certificate2(@"C:\Program Files\OpenSSL-Win64\bin\cert_key.p12", "1234");
            X509SecurityKey x509SecurityKey = new X509SecurityKey(x509Certificate2);
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateIssuerSigningKey = true,
                    ValidAudience = "ZaiZah",
                    ValidIssuer = "ZaiZah",
                    IssuerSigningKey = x509SecurityKey,
                    ClockSkew = TimeSpan.Zero,
                };
            }).AddGoogle("Google", options =>
            {
                options.CallbackPath = new PathString("/acc/google-callback");
                options.ClientId = Configuration.GetSection("google")["clientId"];
                options.ClientSecret = Configuration.GetSection("google")["secret"];
                options.Events = new OAuthEvents
                {
                    OnRemoteFailure = (RemoteFailureContext context) =>
                    {
                        context.Response.Redirect("/home/denied");
                        context.HandleResponse();
                        return Task.CompletedTask;
                    }
                };
            }).AddFacebook(facebookOptions =>
            {
                facebookOptions.AppId = Configuration["facebook:clientId"];
                facebookOptions.AppSecret = Configuration["facebook:secret"];
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.Use(async (ctx, next) =>

            {
                await next();
                if (ctx.Response.StatusCode == 204)
                {
                    ctx.Response.ContentLength = 0;
                }
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors(Builders =>
                Builders.WithOrigins("http://localhost:4200")
                .AllowAnyHeader()
                .AllowAnyMethod());

            app.UseAuthentication();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
