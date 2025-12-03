using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
using System.Text.Json.Serialization;
using System.Text.Json;
using SistemaSubsidios_CASATIC.Services;

var builder = WebApplication.CreateBuilder(args);

// Leer la cadena de conexión desde la variable de entorno (si existe), o desde appsettings.json
var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var connectionString = string.IsNullOrEmpty(envConnection)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : envConnection;

// Verificación de la cadena de conexión correcta
var csb = new MySqlConnectionStringBuilder(connectionString);
Console.WriteLine($"📡 Conectando a base de datos '{csb.Database}' en servidor '{csb.Server}:{csb.Port}' con usuario '{csb.UserID}'.");

// Configuración del DbContext para MySQL
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString, 
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

// Configuración para la autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// 🔥 CONFIGURACIÓN COMPLETA PARA JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase; // ← AHORA FUNCIONARÁ
    });

// Para el manejo de MVC y Razor Pages
builder.Services.AddControllersWithViews(); // MVC
builder.Services.AddRazorPages(); // Razor Pages
//Servicio para correos email
builder.Services.AddSingleton<EmailService>();

//Servicio otp
builder.Services.AddSingleton<OtpService>();

var app = builder.Build();

// Configuración del pipeline de solicitud HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Habilitar la autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

// Mapeo de rutas del controlador MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapeo de Razor Pages (si usas)
app.MapRazorPages();

app.Run();