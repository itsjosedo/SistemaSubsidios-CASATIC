using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SistemaSubsidios_CASATIC.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

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
            var subsidios = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            return View(subsidios);
        }

        // GET: Subsidios/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Subsidios/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Subsidio model)
        {
            _logger.LogInformation("🎯 === INICIANDO CREACIÓN DE SUBSIDIO ===");

            // ✅ REMOVER validación de propiedades [NotMapped]
            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");

            if (!ModelState.IsValid)
            {
                _logger.LogError("❌ MODELSTATE NO VÁLIDO");
                return View(model);
            }

            try
            {
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
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (subsidio == null) return NotFound();
            return View(subsidio);
        }

        // POST: Subsidios/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Subsidio model)
        {
            // ✅ REMOVER validación de propiedades [NotMapped]
            ModelState.Remove("BeneficiarioId");
            ModelState.Remove("Beneficiario");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var subsidioExistente = await _context.Subsidios
                    .Include(s => s.Beneficiarios)  // ← CAMBIADO
                    .FirstOrDefaultAsync(s => s.Id == model.Id);
                    
                if (subsidioExistente == null)
                {
                    ModelState.AddModelError("", "El subsidio que intenta actualizar no existe");
                    return View(model);
                }

                // Actualizar propiedades básicas
                subsidioExistente.NombrePrograma = model.NombrePrograma;
                subsidioExistente.Tipo = model.Tipo;
                subsidioExistente.Monto = model.Monto;
                subsidioExistente.FechaAsignacion = model.FechaAsignacion;
                subsidioExistente.Estado = model.Estado;

                // NOTA: La asignación de beneficiarios se hace con los métodos específicos
                // GestionarBeneficiarios o AsignarBeneficiariosMultiples

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
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();
            return View(subsidio);
        }

        // GET: Subsidios/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subsidio == null) return NotFound();
            return View(subsidio);
        }

        // POST: Subsidios/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO
                .FirstOrDefaultAsync(s => s.Id == id);
                
            if (subsidio != null)
            {
                // Limpiar relaciones muchos-a-muchos antes de eliminar
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
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Reporte General de Subsidios";
            ViewData["FechaReporte"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
            ViewData["TotalSubsidios"] = subsidios.Count;
            ViewData["MontoTotal"] = subsidios.Sum(s => s.Monto).ToString("N2");

            return View(subsidios);
        }

        public async Task<IActionResult> Activos()
        {
            var subsidiosActivos = await _context.Subsidios
                .Include(s => s.Beneficiarios)  // ← CAMBIADO: Beneficiario -> Beneficiarios
                .Where(s => s.Estado == "Activo")
                .OrderByDescending(s => s.Id)
                .ToListAsync();

            ViewData["Title"] = "Subsidios Activos";
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
                .Include(s => s.Beneficiarios)  // ← CAMBIADO
                .ThenInclude(b => b.Entidad)
                .FirstOrDefaultAsync(s => s.Id == id && s.Beneficiarios.Any(b => b.UsuarioId == userId.Value));

            if (subsidio == null)
                return NotFound();

            return View(subsidio);
        }

        // ==============================================
        // 🆕 MÉTODOS NUEVOS PARA ASIGNACIÓN MÚLTIPLE DE BENEFICIARIOS
        // ==============================================

        // GET: Subsidios/GestionarBeneficiarios/5
        public async Task<IActionResult> GestionarBeneficiarios(int id)
        {
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
                return RedirectToAction(nameof(Index));
            }

            // Obtener TODOS los beneficiarios disponibles
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
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);
                
                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
                    return RedirectToAction(nameof(Index));
                }

                // Limpiar beneficiarios actuales
                subsidio.Beneficiarios.Clear();

                // Agregar nuevos beneficiarios seleccionados
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
            var subsidio = await _context.Subsidios
                .Include(s => s.Beneficiarios)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (subsidio == null)
            {
                TempData["ErrorMessage"] = "Subsidio no encontrado";
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
                var subsidio = await _context.Subsidios
                    .Include(s => s.Beneficiarios)
                    .FirstOrDefaultAsync(s => s.Id == id);
                    
                if (subsidio == null)
                {
                    TempData["ErrorMessage"] = "Subsidio no encontrado";
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
        // 📊 MÉTODOS AUXILIARES
        // ==============================================

        // Método para obtener estadísticas de beneficiarios
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
    }
}