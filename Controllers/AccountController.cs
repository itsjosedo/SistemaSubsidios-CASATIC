using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

       [HttpGet]
        public IActionResult Login()
        {
            // SI EL USUARIO YA ESTÁ AUTENTICADO, REDIRIGIR SEGÚN SU ROL
            if (User.Identity.IsAuthenticated)
            {
                var rol = GetRolUsuario();
                var rolNormalizado = rol?.Trim().ToLower() ?? "";
                
                if (rolNormalizado == "entidad" || rolNormalizado == "operador")
                {
                    return RedirectToAction("Dashboard", "Entidad");
                }
                else if (rolNormalizado == "beneficiario")
                {
                    return RedirectToAction("Create", "Beneficiarios");
                }
                else if (rolNormalizado == "admin" || rolNormalizado == "administrador")
                {
                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            
            return View();
        }

        [HttpPost]
public async Task<IActionResult> Login(string correo, string contrasena)
{
    if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(contrasena))
    {
        ViewBag.ErrorMessage = "Por favor, ingrese su correo y contraseña.";
        ViewBag.Correo = correo;
        return View();
    }

    var hash = AuthHelper.Hash(contrasena);
    var usuario = await _db.Usuarios
        .Include(u => u.Entidad)
        .FirstOrDefaultAsync(u => u.Correo == correo && u.Contrasena == hash && u.Estado == "activo");

    if (usuario == null)
    {
        ViewBag.ErrorMessage = "Correo o contraseña incorrectos o usuario inactivo.";
        ViewBag.Correo = correo;
        return View();
    }

    // Crear claims con información adicional
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, usuario.Nombre ?? ""),
        new Claim(ClaimTypes.Email, usuario.Correo ?? ""),
        new Claim(ClaimTypes.Role, usuario.Rol ?? "beneficiario"),
        new Claim("UserId", usuario.Id_Usuario.ToString()),
        new Claim("Rol", usuario.Rol ?? "beneficiario")
    };

    // Agregar EntidadId si existe
    if (usuario.Entidad != null)
    {
        claims.Add(new Claim("EntidadId", usuario.Entidad.Id.ToString()));
    }

    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
    var principal = new ClaimsPrincipal(identity);

    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

    // CORREGIDO: Usar comparación case-insensitive
    var rolNormalizado = usuario.Rol?.Trim() ?? "";
    
    if (rolNormalizado.Equals("admin", StringComparison.OrdinalIgnoreCase) || 
        rolNormalizado.Equals("administrador", StringComparison.OrdinalIgnoreCase))
    {
        return RedirectToAction("Index", "Home");
    }
    else if (rolNormalizado.Equals("entidad", StringComparison.OrdinalIgnoreCase) || 
             rolNormalizado.Equals("operador", StringComparison.OrdinalIgnoreCase))
    {
        return RedirectToAction("Dashboard", "Entidad");
    }
    else if (rolNormalizado.Equals("beneficiario", StringComparison.OrdinalIgnoreCase))
    {
        return RedirectToAction("Create", "Beneficiarios");
    }
    else
    {
        return RedirectToAction("Index", "Home");
    }
}

     [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Logout(string redirectTo)
{
    await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    TempData["FromLogout"] = true;
    
    return redirectTo?.ToLower() switch
    {
        "home" => RedirectToAction("Index", "Home"),
        _ => RedirectToAction("Login", "Account")
    };
}

        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string nombre, string correo, string contrasena, string confirmarContrasena, string rol)
        {
            if (string.IsNullOrWhiteSpace(nombre) ||
                string.IsNullOrWhiteSpace(correo) ||
                string.IsNullOrWhiteSpace(contrasena) ||
                string.IsNullOrWhiteSpace(confirmarContrasena) ||
                string.IsNullOrWhiteSpace(rol))
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

            var hash = AuthHelper.Hash(contrasena);

            var nuevoUsuario = new Usuario
            {
                Nombre = nombre,
                Correo = correo,
                Contrasena = hash,
                Rol = rol ?? "beneficiario",
                Estado = "activo"
            };

            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();

            return RedirectToAction("Login", "Account");
        }
    }
}