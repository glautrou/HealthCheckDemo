using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HealthCheckDemo.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using NetTools;

namespace HealthCheckDemo
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
            services.AddControllersWithViews();

            services.AddHealthChecks()
                .AddCheck<EvenSecondHealthCheck>("even_second")
                .AddUrlGroup(new Uri("https://webnet.fr/"), "Webnet", HealthStatus.Degraded, new[] { "url" })
                //.AddSqlServer("MyConnectionStrings");
                .AddFileExistsHealthCheck(@"PathToFile", "My File", HealthStatus.Degraded, new[] { "server" })
                .AddDirectoryExistsHealthCheck(@"PathToFolder", "My folder", HealthStatus.Degraded, new[] { "server" })
                .AddWindowsScheduledTaskHealthCheck("Scheduled task path", "Machine", "Username", "Password", "Domain", "My job", HealthStatus.Degraded, new[] { "application" });

            services.AddHealthChecksUI();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            //Check if allowed (IP-restricted)
            app.UseHealthCheckRestrictMiddleware();

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.UseHealthChecks("/health-url", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("url"),
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                endpoints.MapHealthChecks("/health");
                endpoints.MapHealthChecksUI();
            });
        }
    }
}
