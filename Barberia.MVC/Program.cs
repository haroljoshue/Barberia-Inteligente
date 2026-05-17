using Barberia.MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using CRUD;
using Barberia.MVC.Areas.Identity.Pages.Account;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

var apiUrl = builder.Configuration["ApiSettings:BaseUrl"]
    ?? "https://barberia-inteligente-api-8809.up.railway.app";

CRUD<Barbero>.EndPoint = $"{apiUrl}/api/barberos";
CRUD<Cita>.EndPoint = $"{apiUrl}/api/citas";
CRUD<Servicio>.EndPoint = $"{apiUrl}/api/servicios";
CRUD<LogAgente>.EndPoint = $"{apiUrl}/api/logagentes";
CRUD<LogSistema>.EndPoint = $"{apiUrl}/api/logsistemas";
CRUD<ApplicationUser>.EndPoint = $"{apiUrl}/api/usuarios";
CRUD<IdentityRole>.EndPoint = $"{apiUrl}/api/roles";

builder.Services.AddHttpClient("BarberiaApi", client =>
{
    client.BaseAddress = new Uri(apiUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddControllersWithViews();
builder.Services.AddSession();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();


var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
app.Run($"http://0.0.0.0:{port}");