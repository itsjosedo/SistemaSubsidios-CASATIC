using SistemaSubsidios_CASATIC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class BeneficiariosController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<BeneficiariosController> _logger;

        public BeneficiariosController(AppDbContext context, ILogger<BeneficiariosController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            ViewBag.UserId = userId;

            var beneficiario = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                ViewBag.PerfilIncompleto = true;
                return View(new BeneficiarioViewModel());
            }

            bool perfilIncompleto = string.IsNullOrWhiteSpace(beneficiario.Direccion)
              || string.IsNullOrWhiteSpace(beneficiario.Telefono)
              || string.IsNullOrWhiteSpace(beneficiario.Dui);

            ViewBag.PerfilIncompleto = perfilIncompleto;

            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Direccion = beneficiario.Direccion,
                Telefono = beneficiario.Telefono,
                EntidadId = beneficiario.EntidadId,
                EstadoSubsidio = beneficiario.EstadoSubsidio,
                EntidadNombre = beneficiario.Entidad?.Nombre
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> CompletarPerfil()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                beneficiario = new Beneficiario
                {
                    UsuarioId = userId.Value
                };
                _context.Beneficiarios.Add(beneficiario);
                await _context.SaveChangesAsync();
            }
            //return RedirectToAction("Index");

            var model = new BeneficiarioViewModel
            {
                Id_Beneficiario = beneficiario.Id_Beneficiario,
                Nombre = beneficiario.Nombre,
                Dui = beneficiario.Dui,
                Direccion = beneficiario.Direccion,
                Telefono = beneficiario.Telefono
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CompletarPerfil(BeneficiarioViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            try
            {
                // ✅ Buscamos el beneficiario del usuario
                var beneficiario = await _context.Beneficiarios
                    .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

                // ✅ Si no existe, lo creamos
                if (beneficiario == null)
                {
                    beneficiario = new Beneficiario
                    {
                        UsuarioId = userId.Value
                    };
                    _context.Beneficiarios.Add(beneficiario);
                }

                // ✅ Aquí sí asignamos los datos del formulario
                beneficiario.Dui = model.Dui?.Trim();
                beneficiario.Telefono = model.Telefono?.Trim();
                beneficiario.Direccion = model.Direccion?.Trim();

                // ✅ Guardamos los cambios
                await _context.SaveChangesAsync();

                Console.WriteLine("Guardado exitoso, redirigiendo...");
                Console.WriteLine($"Dui: {beneficiario.Dui}, Teléfono: {beneficiario.Telefono}, Dirección: {beneficiario.Direccion}");
                TempData["MensajeExito"] = "Tu perfil se ha completado correctamente.";
                return RedirectToAction("Index", "Beneficiarios");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al completar el perfil del beneficiario");
                ModelState.AddModelError("", "Ocurrió un error al guardar los datos.");
                return View(model);
            }
        }

    }
}