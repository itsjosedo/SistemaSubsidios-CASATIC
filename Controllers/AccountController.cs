using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SistemaSubsidios_CASATIC.Services;

using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class AccountController : BaseController
    {
        private readonly EmailService _email;
        private readonly OtpService _otp;

        private readonly AppDbContext _db;

        public AccountController(AppDbContext db, EmailService email, OtpService otp)
        {
            _db = db;
            _email = email;
            _otp = otp;
        }

        [HttpGet]
        public IActionResult Login()
        {
            //Aunteticaciòn y direccionamiento segun roles
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


            if (usuario.Entidad != null)
            {
                claims.Add(new Claim("EntidadId", usuario.Entidad.Id.ToString()));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);


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
                return RedirectToAction("Index", "Beneficiarios");
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
                string.IsNullOrWhiteSpace(confirmarContrasena))

            {
                ViewBag.ErrorMessage = "Todos los campos son obligatorios.";
                return View();
            }

            //Validacion de contraseña
            var regexPassword = new Regex(@"^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$");

            if (!regexPassword.IsMatch(contrasena))
            {
                ViewBag.ErrorMessage = "La contraseña debe tener mínimo 8 caracteres, una mayúscula, un número y un carácter especial.";
                ViewBag.Nombre = nombre;
                ViewBag.Correo = correo;
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
                Estado = "pendiente"
            };

            _db.Usuarios.Add(nuevoUsuario);
            await _db.SaveChangesAsync();

            //Genarar OTP
            var resultado = _otp.Generar(correo);

            //Enviar OTP al correo
            await _email.EnviarCorreo(
                correo,
                "Código de verificación",
                EmailTemplates.OtpTemplate(resultado.Codigo)
            );

            return RedirectToAction("VerificarOtp", "Account", new { correo });
        }

        [HttpGet]
        public IActionResult VerificarOtp(string correo)
        {
            ViewBag.Correo = correo;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerificarOtp(string correo, string codigo)
        {
            if (_otp.Validar(correo, codigo))
            {
                var usuario = await _db.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);

                if (usuario != null)
                {
                    usuario.Estado = "activo";
                    await _db.SaveChangesAsync();
                }
                //Activacion de estado del usuario
                TempData["MensajeExito"] = "Correo verificado correctamente.";
                return RedirectToAction("Login");
            }

            ViewBag.Error = "Código incorrecto o expirado";
            ViewBag.Correo = correo;
            return View();
        }

        //Metodo para reenviar el codigo otp
        [HttpPost]
        public async Task<IActionResult> ReenviarOtp(string correo)
        {
            var codigo = _otp.Generar(correo);
            if (!codigo.Permitido)
            {
                ViewBag.Error = $"Debes esperar {codigo.SegundosRestantes} segundos para reenviar otro código.";
                ViewBag.Correo = correo;
                ViewBag.Cooldown = codigo.SegundosRestantes;
                return View("VerificarOtp");
            }
            await _email.EnviarCorreo(
                correo,
                "Nuevo código OTP",
                EmailTemplates.OtpTemplate(codigo.Codigo!)
            );

            TempData["Mensaje"] = "Se ha enviado un nuevo código.";
            return RedirectToAction("VerificarOtp", new { correo });
        }

        //Metodo para verificar correo y proceder con el cambio de contraseña
        [HttpGet]
        public IActionResult VerificarCorreo()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> VerificarCorreo(string correo)
        {
            var usuario = await _db.Usuarios.AnyAsync(u => u.Correo == correo);

            if (usuario == null)
            {
                ViewBag.ErrorMessage = "Este correo no está registrado.";
                return View();
            }

            return View();
        }
        [HttpGet]
        public IActionResult RestablecerContrasena()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> RestablecerContrasena(string correo, string nuevaContrasena)
        {
            return View();
        }

    }
}