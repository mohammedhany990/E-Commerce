using Demo.BBL.Interfaces;
using Demo.BBL.Repositories;
using Demo.DAL.Data.Contexts;
using Demo.PL.MappingProfile;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Demo.BBL.Services;
using Demo.DAL.Models;
using Demo.PL.Helper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Stripe;

namespace Demo.PL
{
    public class Program
    {
        public static async Task Main(string[] args)
       {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            //builder.Services.AddAuthentication()
            //    .AddFacebook(options =>
            //    {
            //        options.AppId = "";
            //        options.AppSecret = "";
            //    });

            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

            builder.Services.AddRazorPages();

            builder.Services.AddDbContext<ECommerceDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));

            });

            builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<ECommerceDbContext>()
                .AddDefaultTokenProviders();

            


            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = $"/Identity/Account/Login";
                options.LogoutPath = $"/Identity/Account/Logout";
                options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
            });

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout=TimeSpan.FromMinutes(100);
                options.Cookie.HttpOnly=true;
                options.Cookie.IsEssential=true;
            });

            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddAutoMapper(typeof(MappingProfiles));
            builder.Services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            builder.Services.AddScoped(typeof(IEmailSender), typeof(EmailSender));
            builder.Services.Configure<StripeSettings>(builder.Configuration.GetSection("Stripe"));

            var app = builder.Build();


            #region UpdateDatabase

            using var scope = app.Services.CreateScope();
            var service = scope.ServiceProvider;
            var dbContext = service.GetRequiredService<ECommerceDbContext>();
            var loggerFactory = service.GetRequiredService<ILoggerFactory>();

            var userManager = service.GetRequiredService<UserManager<AppUser>>();
            var roleManager = service.GetRequiredService<RoleManager<IdentityRole>>();
            try
            {
                //await ECommerceDbContextSeeding.DataSeedAsync(dbContext);
                await dbContext.Database.MigrateAsync();
               await ECommerceDbContextSeeding.DataSeedAsync(dbContext, roleManager, userManager);
            }
            catch (Exception ex)
            {
                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "an Error has been occured during applying Migrations");
            }

            #endregion


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            StripeConfiguration.ApiKey = builder.Configuration.GetSection("Stripe:Secretkey").Get<string>();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSession();
            app.MapRazorPages();
            app.MapControllerRoute(

                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
