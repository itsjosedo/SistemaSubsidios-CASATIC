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

            var subsidios = await _context.Subsidios
            .Where(s => s.BeneficiarioId == beneficiario.Id_Beneficiario)
            .ToListAsync();

            // ✅ 2️⃣ Enviar los subsidios mediante ViewBag
            ViewBag.Subsidios = subsidios;
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
            {
                var errores = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

                _logger.LogWarning("Errores de validación: {Errores}", string.Join(", ", errores));
                ViewBag.Errores = errores;
                return View(model);
            }

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
                        UsuarioId = userId.Value,

                    };
                    _context.Beneficiarios.Add(beneficiario);
                }

                var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id_Usuario == userId.Value);

                if (usuario != null)
                {
                    beneficiario.Nombre = usuario.Nombre;
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
        // GET: Beneficiarios/Lista (Vista pública para el dashboard)
        public async Task<IActionResult> Lista()
        {
            var beneficiarios = await _context.Beneficiarios
                .Include(b => b.Entidad)
                .Where(b => !string.IsNullOrEmpty(b.Dui) && !string.IsNullOrEmpty(b.Nombre))
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewData["Title"] = "Lista de Beneficiarios";
            return View(beneficiarios);
        }


        //GET: Editar Perfil beneficiario
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiario = await _context.Beneficiarios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
                return RedirectToAction("CompletarPerfil");

            var model = new BeneficiarioViewModel
            {
                Telefono = beneficiario.Telefono,
                Direccion = beneficiario.Direccion
            };

            return View(model);
        }
        // POST: Beneficiarios/EditarPerfil
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(BeneficiarioViewModel model)
        {
            Console.WriteLine("Método POST EditarPerfil ejecutado");

            var userId = GetUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //Buscamos al beneficiario ANTES de validar
            var beneficiario = await _context.Beneficiarios
                .FirstOrDefaultAsync(b => b.UsuarioId == userId.Value);

            if (beneficiario == null)
            {
                return RedirectToAction("CompletarPerfil");
            }

            // ✅ Validamos solo lo que realmente se está editando
            // (para evitar errores por otros campos requeridos que no se muestran)
            ModelState.Remove("Nombre");
            ModelState.Remove("Dui");
            ModelState.Remove("Correo");

            if (!ModelState.IsValid)
            {
                // ✅ Reasignamos datos actuales para que no se pierdan en la vista
                model.Telefono = beneficiario.Telefono;
                model.Direccion = beneficiario.Direccion;
                return View(model);
            }

            // ✅ Guardamos los cambios permitidos
            beneficiario.Telefono = model.Telefono?.Trim();
            beneficiario.Direccion = model.Direccion?.Trim();

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Perfil actualizado correctamente.";
            return RedirectToAction("Index");
        }


    }
}