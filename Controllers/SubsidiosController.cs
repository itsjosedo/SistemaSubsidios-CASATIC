using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaSubsidios_CASATIC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace SistemaSubsidios_CASATIC.Controllers
{
    public class SubsidiosController : BaseController
    {
        private readonly AppDbContext _context;
        private readonly ILogger<SubsidiosController> _logger;

        public SubsidiosController(AppDbContext context, ILogger<SubsidiosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: Subsidios
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            IQueryable<Subsidio> subsidiosQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .OrderByDescending(s => s.Id);

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                subsidiosQuery = subsidiosQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var subsidios = await subsidiosQuery.ToListAsync();

            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";
            ViewBag.UserName = User.Identity?.Name;
            ViewBag.TieneSubsidios = subsidios.Any();

            return View(subsidios);
        }

        // GET: Subsidios/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                var tieneSubsidios = _context.Subsidios.Any(s => s.UsuarioCreacionId == userId.Value.ToString());
                ViewBag.TieneSubsidios = tieneSubsidios;
                ViewBag.UserId = userId.Value.ToString();
            }
            else
            {
                ViewBag.TieneSubsidios = true;
                ViewBag.UserId = "0";
            }

            return View();
        }

        // POST: Subsidios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subsidio model)
        {
            _logger.LogInformation("🎯 === INICIANDO CREACIÓN DE SUBSIDIO ===");

            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");
            ModelState.Remove("UsuarioCreacionId");

            if (!ModelState.IsValid)
            {
                _logger.LogError("❌ MODELSTATE NO VÁLIDO");
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    _logger.LogError($"Error: {error.ErrorMessage}");
                }
                return View(model);
            }

            try
            {
                var userId = GetUserId();
                if (userId.HasValue)
                {
                    model.UsuarioCreacionId = userId.Value.ToString();
                }
                else
                {
                    model.UsuarioCreacionId = "0";
                }

                _context.Subsidios.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' creado correctamente. Ahora puede asignar beneficiarios.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear subsidio");
                ModelState.AddModelError("", "Error al crear el subsidio: " + ex.Message);
                return View(model);
            }
        }

        // GET: Subsidios/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para editar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            return View(subsidio);
        }

        // POST: Subsidios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subsidio model)
        {
            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidioExistente = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (subsidioExistente == null)
                {
                    ModelState.AddModelError("", "El subsidio que intenta actualizar no existe");
                    return View(model);
                }

                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidioExistente.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para editar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                subsidioExistente.NombrePrograma = model.NombrePrograma;
                subsidioExistente.Tipo = model.Tipo;
                subsidioExistente.Monto = model.Monto;
                subsidioExistente.FechaAsignacion = model.FechaAsignacion;
                subsidioExistente.Estado = model.Estado;
                subsidioExistente.FechaExpiracion = model.FechaExpiracion;

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Subsidio '{model.NombrePrograma}' actualizado correctamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar subsidio");
                ModelState.AddModelError("", "Error al actualizar el subsidio: " + ex.Message);
                return View(model);
            }
        }

        // GET: Subsidios/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para ver este subsidio";
                return RedirectToAction(nameof(Index));
            }

            return View(subsidio);
        }

        // GET: Subsidios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para eliminar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            return View(subsidio);
        }

        // POST: Subsidios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio != null)
            {
                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para eliminar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                subsidio.Beneficiarios.Clear();
                _context.Subsidios.Remove(subsidio);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Subsidios/Reporte
        public async Task<IActionResult> Reporte()
        {
            var subsidios = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Reporte General de Subsidios";
            ViewData["FechaReporte"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewData["TotalSubsidios"] = subsidios.Count;
            ViewData["MontoTotal"] = subsidios.Sum(s => s.Monto).ToString("N2");

            return View(subsidios);
        }

        // GET: Subsidios/Activos
        public async Task<IActionResult> Activos()
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            if (userId == null)
            {
                TempData["ErrorMessage"] = "Debe iniciar sesión para ver los subsidios activos";
                return RedirectToAction("Login", "Account");
            }

            IQueryable<Subsidio> subsidiosQuery = _context.Subsidios
                .Include(s => s.Beneficiarios)
                .Where(s => s.Estado == "Activo");

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue)
            {
                subsidiosQuery = subsidiosQuery.Where(s => s.UsuarioCreacionId == userId.Value.ToString());
            }

            var subsidiosActivos = await subsidiosQuery
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Subsidios Activos de Mi Entidad";
            ViewBag.EsEntidad = rolUsuario?.ToLower() == "entidad";
            return View(subsidiosActivos);
        }

        // Mostrar detalles del subsidio en beneficiario
        [HttpGet]
        public async Task<IActionResult> Detalles(int id)
        {
            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .ThenInclude(b => b.Entidad)
                .FirstOrDefaultAsync(s => s.Id == id && s.Beneficiarios.Any(b => b.UsuarioId == userId.Value));

            if (subsidio == null)
                return NotFound();

            return View(subsidio);
        }

        // ==============================================
        // 🆕 MÉTODOS PARA ASIGNACIÓN MÚLTIPLE DE BENEFICIARIOS
        // ==============================================

        // GET: Subsidios/GestionarBeneficiarios/5
        public async Task<IActionResult> GestionarBeneficiarios(int id)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                return RedirectToAction(nameof(Index));
            }

            var todosBeneficiarios = await _context.Beneficiarios
                .Where(b => !string.IsNullOrEmpty(b.Dui) && !string.IsNullOrEmpty(b.Nombre))
                .OrderBy(b => b.Nombre)
                .ToListAsync();

            ViewBag.BeneficiariosDisponibles = todosBeneficiarios;
            ViewBag.BeneficiariosAsignados = subsidio.Beneficiarios.Select(b => b.Id_Beneficiario).ToList();

            return View(subsidio);
        }

        // POST: Subsidios/GestionarBeneficiarios/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GestionarBeneficiarios(int id, List<int> beneficiariosIds)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                subsidio.Beneficiarios.Clear();

                if (beneficiariosIds != null && beneficiariosIds.Any())
                {
                    var beneficiarios = await _context.Beneficiarios
                        .Where(b => beneficiariosIds.Contains(b.Id_Beneficiario))
                        .ToListAsync();

                    foreach (var beneficiario in beneficiarios)
                    {
                        subsidio.Beneficiarios.Add(beneficiario);
                    }
                }

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Beneficiarios actualizados correctamente para el subsidio '{subsidio.NombrePrograma}'";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al gestionar beneficiarios");
                TempData["ErrorMessage"] = "Error al gestionar los beneficiarios: " + ex.Message;
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
        }

        // GET: Subsidios/QuitarBeneficiario/5
        public async Task<IActionResult> QuitarBeneficiario(int id, int beneficiarioId)
        {
            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                return RedirectToAction(nameof(Index));
            }

            var beneficiario = subsidio.Beneficiarios.FirstOrDefault(b => b.Id_Beneficiario == beneficiarioId);
            if (beneficiario == null)
            {
                TempData["WarningMessage"] = "Beneficiario no encontrado en este subsidio";
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }

            ViewBag.Beneficiario = beneficiario;
            return View(subsidio);
        }

        // POST: Subsidios/QuitarBeneficiario/5
        [HttpPost, ActionName("QuitarBeneficiario")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuitarBeneficiarioConfirmed(int id, int beneficiarioId)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para gestionar beneficiarios de este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                var beneficiario = subsidio.Beneficiarios.FirstOrDefault(b => b.Id_Beneficiario == beneficiarioId);
                if (beneficiario != null)
                {
                    subsidio.Beneficiarios.Remove(beneficiario);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Beneficiario '{beneficiario.Nombre}' removido correctamente del subsidio '{subsidio.NombrePrograma}'";
                }

                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al quitar beneficiario");
                TempData["ErrorMessage"] = "Error al quitar el beneficiario: " + ex.Message;
                return RedirectToAction(nameof(GestionarBeneficiarios), new { id });
            }
        }

        // ==============================================
        //  MÉTODOS PARA RENOVACIÓN DE SUBSIDIOS
        // ==============================================

        // GET: Subsidios/Renovar/5
        [HttpGet]
        public async Task<IActionResult> Renovar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = GetUserId();
            var rolUsuario = GetRolUsuario();

            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (subsidio == null)
            {
                return NotFound();
            }

            if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
            {
                TempData["ErrorMessage"] = "No tiene permisos para renovar este subsidio";
                return RedirectToAction(nameof(Index));
            }

            if (subsidio.Estado.ToLower() != "activo" && subsidio.Estado.ToLower() != "pendiente")
            {
                TempData["WarningMessage"] = $"No se puede renovar un subsidio con estado '{subsidio.Estado}'. Solo subsidios activos o pendientes pueden ser renovados.";
                return RedirectToAction(nameof(Details), new { id = subsidio.Id });
            }

            ViewBag.FechaMinima = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");
            ViewBag.FechaSugerida = DateTime.Now.AddYears(1).ToString("yyyy-MM-dd");

            return View(subsidio);
        }

        // POST: Subsidios/Renovar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Renovar(int id, DateTime nuevaFechaExpiracion)
        {
            try
            {
                var userId = GetUserId();
                var rolUsuario = GetRolUsuario();

                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);

                if (subsidio == null)
                {
                    return NotFound();
                }

                if (rolUsuario?.ToLower() == "entidad" && userId.HasValue && subsidio.UsuarioCreacionId != userId.Value.ToString())
                {
                    TempData["ErrorMessage"] = "No tiene permisos para renovar este subsidio";
                    return RedirectToAction(nameof(Index));
                }

                // Validar fecha
                if (nuevaFechaExpiracion <= DateTime.Now)
                {
                    TempData["ErrorMessage"] = "La fecha de expiración debe ser posterior a la fecha actual";
                    return RedirectToAction(nameof(Renovar), new { id });
                }

                if (nuevaFechaExpiracion <= subsidio.FechaExpiracion)
                {
                    TempData["ErrorMessage"] = "La nueva fecha de expiración debe ser posterior a la fecha de expiración actual";
                    return RedirectToAction(nameof(Renovar), new { id });
                }

                // Guardar la fecha anterior para historial
                var fechaExpiracionAnterior = subsidio.FechaExpiracion;

                // Actualizar fechas
                subsidio.FechaExpiracion = nuevaFechaExpiracion;
                subsidio.FechaRenovacion = DateTime.Now;
                subsidio.ProximaRenovacion = CalcularProximaRenovacionSugerida(nuevaFechaExpiracion);
                subsidio.Estado = "Activo"; // Cambiar estado a Activo si estaba Pendiente

                // Registrar en el log
                _logger.LogInformation($"Subsidio ID {subsidio.Id} renovado. Fecha anterior: {fechaExpiracionAnterior:dd/MM/yyyy}, Nueva fecha: {nuevaFechaExpiracion:dd/MM/yyyy}");

                _context.Update(subsidio);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"✅ Subsidio '{subsidio.NombrePrograma}' renovado correctamente. Nueva fecha de expiración: {nuevaFechaExpiracion:dd/MM/yyyy}";
                return RedirectToAction(nameof(Details), new { id = subsidio.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al renovar subsidio");
                TempData["ErrorMessage"] = "Error al renovar el subsidio: " + ex.Message;
                return RedirectToAction(nameof(Renovar), new { id });
            }
        }

        // Método auxiliar para calcular próxima renovación sugerida
        private DateTime? CalcularProximaRenovacionSugerida(DateTime nuevaFechaExpiracion)
        {
            // Sugerir renovación 30 días antes de la expiración
            return nuevaFechaExpiracion.AddDays(-30);
        }

        // ==============================================
        // 🔧 MÉTODO PARA ASIGNAR SUBSIDIOS EXISTENTES
        // ==============================================

        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AsignarSubsidiosExistente()
        {
            string usuarioCasaticId = "3";

            var subsidiosExistentes = await _context.Subsidios.ToListAsync();

            int contador = 0;
            foreach (var subsidio in subsidiosExistentes)
            {
                if (string.IsNullOrEmpty(subsidio.UsuarioCreacionId))
                {
                    subsidio.UsuarioCreacionId = usuarioCasaticId;
                    contador++;
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Se asignaron {contador} subsidios existentes al usuario 'Casatic'";
            return RedirectToAction(nameof(Index));
        }

        // ==============================================
        // 📊 MÉTODOS AUXILIARES
        // ==============================================

        public JsonResult GetEstadisticasBeneficiarios()
        {
            var estadisticas = new
            {
                TotalSubsidios = _context.Subsidios.Count(),
                SubsidiosConBeneficiarios = _context.Subsidios.Count(s => s.Beneficiarios.Any()),
                TotalBeneficiariosAsignados = _context.Subsidios.Sum(s => s.Beneficiarios.Count)
            };

            return Json(estadisticas);
        }

        // Método auxiliar para obtener fecha sugerida de renovación
        private DateTime CalcularFechaSugeridaRenovacion(Subsidio subsidio)
        {
            // Si ya ha sido renovado antes, usar el mismo período
            if (subsidio.FechaRenovacion.HasValue)
            {
                var ultimaDuracion = (subsidio.FechaExpiracion - subsidio.FechaRenovacion.Value).Days;
                return DateTime.Now.AddDays(ultimaDuracion);
            }

            // Si no, usar 1 año por defecto
            return DateTime.Now.AddYears(1);
        }
    }
}