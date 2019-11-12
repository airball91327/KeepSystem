using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using EDIS.Data;
using EDIS.Areas.BMED.Data;
using EDIS.Models;
using EDIS.Services;
using EDIS.Repositories;
using EDIS.Areas.BMED.Repositories;
using EDIS.Models.RepairModels;
using EDIS.Models.Identity;
using EDIS.Models.LocationModels;
using Microsoft.Extensions.FileProviders;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EDIS.Fliters;
using Microsoft.AspNetCore.Mvc;

namespace EDIS
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
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AzureConnection")));//AzureConnection//EdisConnection

            services.AddDbContext<BMEDDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("AzureConnection")));//AzureConnection//EdisConnection

            services.AddScoped<IRepository<RepairModel, string>, RepairRepository>();
            services.AddScoped<IRepository<RepairDtlModel, string>, RepairDtlRepository>();
            services.AddScoped<IRepository<RepairFlowModel, string[]>, RepairFlowRepository>();
            services.AddScoped<IRepository<AppUserModel, int>, AppUserRepository>();
            services.AddScoped<IRepository<DepartmentModel, string>, DepartmentRepository>();
            services.AddScoped<IRepository<DocIdStore, string[]>, DocIdStoreRepository>();
            services.AddScoped<IRepository<BuildingModel, int>, BuildingRepository>();
            services.AddScoped<IRepository<FloorModel, string[]>, FloorRepository>();
            services.AddScoped<IRepository<RepairEmpModel, string[]>, RepairEmpRepository>();
            services.AddScoped<IRepository<AppRoleModel, int>, AppRoleRepository>();

            services.AddScoped<BMEDIRepository<Areas.BMED.Models.RepairModels.RepairModel, string>, BMEDRepairRepository>();
            services.AddScoped<BMEDIRepository<Areas.BMED.Models.RepairModels.RepairDtlModel, string>, BMEDRepairDtlRepository>();
            services.AddScoped<BMEDIRepository<Areas.BMED.Models.RepairModels.RepairFlowModel, string[]>, BMEDRepairFlowRepository>();
            services.AddScoped<BMEDIRepository<Areas.BMED.Models.RepairModels.RepairEmpModel, string[]>, BMEDRepairEmpRepository>();

            services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.FromHours(10));
            //services.AddIdentity<ApplicationUser, IdentityRole>()
            services.AddIdentity<ApplicationUser, ApplicationRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddEntityFrameworkStores<BMEDDbContext>()
                .AddRoleStore<ApplicationRole>()
                .AddUserStore<ApplicationUser>()
                .AddDefaultTokenProviders()
                .AddUserManager<CustomUserManager>()
                .AddRoleManager<CustomRoleManager>();

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "Asp.Net.Core.Identity.RepairSys";
                options.ExpireTimeSpan = TimeSpan.FromHours(2);
                options.SlidingExpiration = true;   //動態更新Cookie的過期時間，超過50%自動更新，無設定時預設值為true
            });

            //
            services.AddScoped<UserManager<ApplicationUser>, CustomUserManager>();
            services.AddScoped<RoleManager<ApplicationRole>, CustomRoleManager>();
            services.AddScoped<CustomSignInManager>();
            // 
            services.AddTransient<IUserStore<ApplicationUser>, CustomUserStore>();
            services.AddTransient<IRoleStore<ApplicationRole>, CustomRoleStore>();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            //services.AddMvc();
            services.AddMvc(options =>
            {
                options.Filters.Add<MyErrorHandlerFilter>();
            });

            //XML formatter.
            services.AddMvc().AddXmlSerializerFormatters();

            //Disable Automatic Model State Validation in ASP.NET Core 2.1 WebApi
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseBrowserLink();
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(
            //        Path.Combine(Directory.GetCurrentDirectory(), "Files")),
            //    RequestPath = "/Files"
            //});

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                name: "areas",
                template: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

                routes.MapRoute(
                name: "default",
                template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
