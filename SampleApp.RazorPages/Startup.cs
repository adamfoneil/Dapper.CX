using Dapper.CX.SqlServer.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelSync.Models;
using SampleApp.Models;
using SampleApp.RazorPages.Data;
using SampleApp.RazorPages.Services;
using System;

namespace SampleApp.RazorPages
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("Default");

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));

            services
                .AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddScoped<ISession>((sp) => new UserAppData(sp));

            services.AddDapperCX(
                connectionString, 
                //sp => sp.GetAspNetUser<UserProfile>(connectionString), 
                sp => sp.GetAspNetUserWithRoles<UserProfile>(connectionString, "profile"),
                (id) => Convert.ToInt32(id));

            services.AddCustomUserStore(connectionString);

            services.AddRazorPages();
            services.AddControllersWithViews();
            services.AddChangeTracking(connectionString, new DataModel());
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

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapDefaultControllerRoute();
            });
        }
    }
}
