using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
  // Asegúrate que sea tu namespace correcto

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        // ========================
        // VISTA DE LOGIN (GET)
        // ========================
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // ========================
        // PROCESO DE LOGIN (POST)
        // ========================
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
            {
                ViewBag.ErrorMessage = "Por favor, ingrese su correo y contraseña.";
                ViewBag.Correo = correo; // Mantener el correo ingresado
                return View();
            }

            var hash = AuthHelper.Hash(contrasena);
            var usuario = await _db.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo && u.Contrasena == hash);

            if (usuario == null)
            {
                ViewBag.ErrorMessage = "Correo o contraseña incorrectos.";
                ViewBag.Correo = correo; // Mantener el correo ingresado
                return View();
            }

            // Resto del código de autenticación...
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nombre ?? ""),
        new Claim(ClaimTypes.Email, usuario.Correo),
        new Claim(ClaimTypes.Role, usuario.Rol ?? "beneficiario")
    };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    
        switch (usuario.Rol?.ToLower())
            {
                case "admin":
                    return RedirectToAction("Index", "Home");
                case "beneficiario":
                    return RedirectToAction("Create", "Beneficiarios");
                case "entidad":
                    return RedirectToAction("Index", "Entidades");
                default:
                    return RedirectToAction("Index", "Home"); 
            }

}
        // ========================
        // LOGOUT
        // ========================
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // ========================
        // VISTA DE ACCESO DENEGADO
        // ========================
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ========================
        // VISTA DE REGISTRO (GET)
        // ========================
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // ========================
        // PROCESO DE REGISTRO (POST)
        // ========================
        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string correo, string contrasena, string confirmarContrasena)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena) ||
                string.IsNullOrWhiteSpace(confirmarContrasena))
            {
                ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
                return View();
            }

            if (contrasena != confirmarContrasena)
            {
                ViewBag.ErrorMessage = "Las contraseñas no coinciden.";
                return View();
            }

            var existe = await _db.Usuarios.AnyAsync(u => u.Correo == correo);
            if (existe)
            {
                ViewBag.ErrorMessage = "Este correo ya está registrado.";
                return View();
            }

            // Hashear la contraseña
            var hash = AuthHelper.Hash(contrasena);

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = hash,
                Rol = "beneficiario",  // Puedes ajustar esto según tus necesidades
                Estado = "activo"
            };

            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();

            // Redirigir al login después de registrarse
            return RedirectToAction("Login", "Account");
        }
    }
}
