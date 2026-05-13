using Barberia.MVC.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ModelosBarberia;
using CRUD;
using Barberia.MVC.Areas.Identity.Pages.Account;

var builder = WebApplication.CreateBuilder(args);

// DbContext para Identity usando Supabase
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString)); // <- Npgsql porque es PostgreSQL

// Configuración de Identity
// Configuración de Identity con ApplicationUser
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // si usas roles
.AddEntityFrameworkStores<ApplicationDbContext>();



// ENDPOINTS API
CRUD<Barbero>.EndPoint   = "https://localhost:7122/api/barberos";
CRUD<Cita>.EndPoint      = "https://localhost:7122/api/citas";
CRUD<Servicio>.EndPoint  = "https://localhost:7122/api/servicios";
CRUD<LogAgente>.EndPoint = "https://localhost:7122/api/logagentes";
CRUD<LogSistema>.EndPoint= "https://localhost:7122/api/logsistemas";
CRUD<ApplicationUser>.EndPoint = "https://localhost:7122/api/usuarios";
CRUD<IdentityRole>.EndPoint = "https://localhost:7122/api/roles";

// Registrar IHttpClientFactory para consumir la API
builder.Services.AddHttpClient("BarberiaApi", client =>
{
    client.BaseAddress = new Uri("https://localhost:7122/"); // URL de tu API
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

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();   // <- necesario para login
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();       // <- necesario para las páginas de Identity

app.Run();
