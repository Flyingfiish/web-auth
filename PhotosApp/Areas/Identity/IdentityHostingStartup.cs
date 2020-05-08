using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PhotosApp.Areas.Identity.Data;
using PhotosApp.Services;

[assembly: HostingStartup(typeof(PhotosApp.Areas.Identity.IdentityHostingStartup))]
namespace PhotosApp.Areas.Identity
{
    public class IdentityHostingStartup : IHostingStartup
    {

        public void Configure(IWebHostBuilder builder)
        {
            builder.ConfigureServices((context, services) =>
            {
                services.AddScoped<IPasswordHasher<PhotoAppUser>, SimplePasswordHasher<PhotoAppUser>>();

                services.Configure<IdentityOptions>(options =>
                {
                    // Default Password settings.
                    options.Password.RequireDigit = false;
                    
                    options.Password.RequireLowercase = true;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = true;
                    options.Password.RequiredLength = 6;
                    options.Password.RequiredUniqueChars = 1;

                    options.SignIn.RequireConfirmedAccount = false;
                    options.SignIn.RequireConfirmedEmail = false;
                    options.SignIn.RequireConfirmedPhoneNumber = false;
                    
                });

                services.ConfigureApplicationCookie(options =>
                {
                    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
                    options.Cookie.Name = "YourAppCookieName";
                    options.Cookie.HttpOnly = true;
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
                    options.LoginPath = "/Identity/Account/Login";
                    // ReturnUrlParameter requires 
                    //using Microsoft.AspNetCore.Authentication.Cookies;
                    options.ReturnUrlParameter = CookieAuthenticationDefaults.ReturnUrlParameter;
                    options.SlidingExpiration = true;
                    options.Cookie.Name = "PhotosApp.Auth";
                });

                /*services.Configure<PasswordHasherOptions>(options =>
                {
                    options.CompatibilityMode = PasswordHasherCompatibilityMode.IdentityV3;
                    
                    options.IterationCount = 12000;
                });*/


                services.AddDbContext<UsersDbContext>(options =>
                    options.UseSqlite(
                        context.Configuration.GetConnectionString("UsersDbContextConnection")));

                services.AddDefaultIdentity<PhotoAppUser>(options => options.SignIn.RequireConfirmedAccount = true)
                    .AddEntityFrameworkStores<UsersDbContext>()
                .AddPasswordValidator<UsernameAsPasswordValidator<PhotoAppUser>>();
            });
        }
    }
}