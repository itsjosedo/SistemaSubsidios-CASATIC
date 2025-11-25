using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
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
        new MySqlServerVersion(new Version(8, 0, 33))  // Especificar la versión de MySQL que estás usando
    )
);

// Configuración para la autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login"; // Ruta del login
        options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta para acceso denegado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
        options.SlidingExpiration = true; // Extiende la sesión si el usuario está activo
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
