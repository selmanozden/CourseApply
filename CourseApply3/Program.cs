using Serilog;
using CourseApply3.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/myapp.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();


builder.Services.AddControllersWithViews();



builder.Services.AddAuthentication()

.AddCookie("AdminScheme", options =>
{
    options.Cookie.Name = "AdminAuthCookie";
    options.LoginPath = "/Admin/AdminLogin";
    options.AccessDeniedPath = "/Admin/AdminLogin";
})
.AddCookie("UserScheme", options =>
{
    options.Cookie.Name = "UserAuthCookie";
    options.LoginPath = "/Home/Login";
    options.AccessDeniedPath = "/Home/Login";
});

// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(CookieAuthenticationDefaults.AuthenticationScheme, policy =>
    {
        policy.AuthenticationSchemes.Add(CookieAuthenticationDefaults.AuthenticationScheme);
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("AdminScheme");
        policy.RequireAuthenticatedUser();
    });

    options.AddPolicy("UserPolicy", policy =>
    {
        policy.AuthenticationSchemes.Add("UserScheme");
        policy.RequireAuthenticatedUser();
    });
});
//builder.Services.AddAuthorization();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<CourseDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401)
    {
        context.HttpContext.Response.Redirect("/Account/Login");
    }
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseAuthentication(); // Kimlik doðrulama middleware'i
app.UseAuthorization();  // Yetkilendirme middleware'i

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
//app.Use(async (context, next) =>
//{
//    // Eðer kullanýcý anasayfadaysa, otomatik olarak Login sayfasýna yönlendir
//    if (context.Request.Path == "/")
//    {
//        context.Response.Redirect("/Home/Login");
//        return;
//    }

//    await next();
//});

app.Run();