using ChatApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using ChatApp.Hubs;

namespace ChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddSingleton<MongoDbService>();

            // Add session services with a timeout setting
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); // Set session timeout
                options.Cookie.HttpOnly = true;                 // Ensure cookies are HttpOnly
                options.Cookie.IsEssential = true;              // Essential for GDPR compliance
            });

            // Configure authentication with cookies
            builder.Services.AddAuthentication("CookieAuth")
                .AddCookie("CookieAuth", options =>
                {
                    options.LoginPath = "/Home/Login";          // Set login path
                    options.AccessDeniedPath = "/Home/AccessDenied"; // Optional: Access denied path
                });

            //builder.Services.AddSignalR();

            var app = builder.Build();

			if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }
			//app.MapHub<ChatHub>("/chathub");
            app.UseStaticFiles();

            app.UseRouting();
			
			// Add authentication, authorization, and session middleware
			app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
			


			app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
