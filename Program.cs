using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyIdentityApp.Data;
using MyIdentityApp.DataInitializer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;  // DotNetEnv kütüphanesini ekledik

var builder = WebApplication.CreateBuilder(args);

// .env dosyasını yükleme
Env.Load();

// Add services to the container.
/* ==================== IDENTITY CONFIGURATION ==================== */

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
});

// Authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/api/account/login"; // API path
        options.LogoutPath = "/api/account/logout"; // API path
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Oturum süresi
    });

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

/* ==================== EMAIL SERVICE CONFIGURATION ==================== */

// SMTP ayarlarını .env dosyasından okuma
var smtpEmail = Environment.GetEnvironmentVariable("SMTP_EMAIL");
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD");

// E-posta gönderme servisini DI container'a ekleme
builder.Services.AddTransient<EmailService>();


/* ==================== ==================== */

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await DataInitializer.Initialize(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();
