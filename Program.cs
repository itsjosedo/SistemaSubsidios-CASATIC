using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using MySqlConnector;
using System.Text.Json.Serialization;
using System.Text.Json;
using SistemaSubsidios_CASATIC.Services;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. CONFIGURACIÓN DE BASE DE DATOS
// ==========================================

var envConnection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
var connectionString = string.IsNullOrEmpty(envConnection)
    ? builder.Configuration.GetConnectionString("DefaultConnection")
    : envConnection;

connectionString ??= "";

try 
{
    var csb = new MySqlConnectionStringBuilder(connectionString);
    Console.WriteLine($"📡 Conectando a base de datos '{csb.Database}' en servidor '{csb.Server}:{csb.Port}' con usuario '{csb.UserID}'.");
}
catch
{
    Console.WriteLine("⚠️ Advertencia: No se pudo analizar la cadena de conexión para el log.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(
        connectionString, 
        new MySqlServerVersion(new Version(8, 0, 33))
    )
);

// ==========================================
// 2. CONFIGURACIÓN DE AUTENTICACIÓN
// ==========================================
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

// ==========================================
// 3. REGISTRO DE SERVICIOS (AQUÍ ESTABA EL ERROR)
// ==========================================

// EmailService se crea cada vez que se necesita (Transient)
builder.Services.AddTransient<EmailService>();

// 🔥 OtpService DEBE ser Singleton para que no olvide los códigos generados
builder.Services.AddSingleton<OtpService>(); 

//Servicio de logs
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<LogService>();

// ==========================================
// 4. CONFIGURACIÓN MVC Y JSON
// ==========================================
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// Para el manejo de MVC y Razor Pages
builder.Services.AddControllersWithViews(); // MVC
builder.Services.AddRazorPages(); // Razor Pages

var app = builder.Build();

// ==========================================
// 5. PIPELINE DE SOLICITUDES HTTP
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
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